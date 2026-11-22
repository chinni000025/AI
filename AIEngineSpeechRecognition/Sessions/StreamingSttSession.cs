using AIEngineConnectivity.SpeechRecognition;
using AIEngineSpeechRecognition.Audio;
using AIEngineSpeechRecognition.Buffers;
using AIEngineSpeechRecognition.Configuration;
using AIEngineSpeechRecognition.Transcription;
using AIEngineSpeechRecognition.Vad;
using System.Threading.Channels;

namespace AIEngineSpeechRecognition.Sessions;

public enum SessionSpeechState { Idle, Speaking, Finalizing }

public sealed class StreamingSttSession : IStreamingSttSession
{
    public Guid SessionId { get; }
    private readonly SpeechRecognitionOptions _options;
    private readonly OpusAudioDecoder _decoder;
    private readonly StreamingAudioResampler _resampler;
    private readonly PcmFrameAccumulator _accumulator;
    private readonly SileroOnnxVadRunner _vadRunner;
    private readonly FloatRingBuffer _preRollBuffer;
    private readonly UtteranceAudioBuffer _utteranceBuffer;
    private readonly WhisperNetTranscriber _transcriber;

    private readonly Channel<TranscriptEventDto> _channel;
    private readonly float[] _decodeFloatBuffer = new float[48000 * 120 / 1000]; // up to 120ms at 48k
    private readonly float[] _resampleFloatBuffer = new float[16000 * 120 / 1000]; // up to 120ms at 16k

    // Serializes concurrent RTP packet callbacks so _accumulator/_state are single-threaded
    private readonly SemaphoreSlim _processingLock = new SemaphoreSlim(1, 1);

    private SessionSpeechState _state = SessionSpeechState.Idle;
    private long _utteranceId = 0;
    private long _revision = 0;
    private int _consecutiveSpeechFrames = 0;
    private int _silenceSamples = 0;
    private int _samplesSinceLastInterim = 0;
    private Task? _inFlightInterimTask = null;

    public StreamingSttSession(Guid sessionId, SpeechRecognitionOptions options,
        WhisperNetTranscriber transcriber, SileroOnnxVadRunner vadRunner)
    {
        SessionId = sessionId;
        _options = options;
        _transcriber = transcriber;
        _vadRunner = vadRunner;

        _decoder = new OpusAudioDecoder(options.InputSampleRate, options.Channels);
        _resampler = new StreamingAudioResampler(options.InputSampleRate, options.TargetSampleRate);
        _accumulator = new PcmFrameAccumulator(options.Vad.FrameSize);
        _preRollBuffer = new FloatRingBuffer(options.Vad.PreRollMilliseconds * 16);
        _utteranceBuffer = new UtteranceAudioBuffer(options.Utterance.MaximumSamples);

        _channel = Channel.CreateBounded<TranscriptEventDto>(new BoundedChannelOptions(32)
        {
            SingleWriter = false,  // Both FinalizeUtteranceAsync and RunInterimInferenceAsync can write
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public async ValueTask PushEncodedAudioPacketAsync(ReadOnlyMemory<byte> payload, AudioPacketMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        await _processingLock.WaitAsync(cancellationToken);
        try
        {
            // 1. Decode Opus 48kHz
            int decodedSamples = _decoder.DecodeToFloat(payload.Span, _decodeFloatBuffer);
            if (decodedSamples <= 0) return;

            // 2. Resample 48kHz -> 16kHz
            int resampledSamples = _resampler.Resample3To1(_decodeFloatBuffer.AsSpan(0, decodedSamples), _resampleFloatBuffer);
            var pcm16k = _resampleFloatBuffer.AsSpan(0, resampledSamples);

            // 3. Accumulate into 512-sample frames & execute VAD
            _accumulator.Append(pcm16k, frame =>
            {
                ProcessVadFrame(frame, cancellationToken);
            });
        }
        catch (OperationCanceledException) { }
        catch
        {
            throw;
        }
        finally
        {
            _processingLock.Release();
        }
    }

    private int _vadLogCounter = 0;
    private void ProcessVadFrame(ReadOnlyMemory<float> frame, CancellationToken ct)
    {
        var span = frame.Span;
        float sumSq = 0f;
        float peak = 0f;
        for (int i = 0; i < span.Length; i++)
        {
            float v = span[i];
            float abs = Math.Abs(v);
            if (abs > peak) peak = abs;
            sumSq += v * v;
        }
        float rms = (float)Math.Sqrt(sumSq / span.Length);

        float speechProbability = _vadRunner.Evaluate(frame);
        _vadLogCounter++;

        // Hybrid detection: neural Silero VAD + acoustic energy (peak/RMS)
        bool isSpeechStart = speechProbability >= _options.Vad.SpeechStartThreshold || peak >= 0.08f || rms >= 0.02f;
        bool isSpeechContinue = speechProbability >= _options.Vad.SpeechContinueThreshold || peak >= 0.05f || rms >= 0.012f;

        if (_state == SessionSpeechState.Idle)
        {
            _preRollBuffer.Write(frame.Span);

            if (isSpeechStart)
            {
                _consecutiveSpeechFrames++;
            }
            else
            {
                _consecutiveSpeechFrames = 0;
            }

            if (_consecutiveSpeechFrames >= _options.Vad.StartConfirmationFrames)
            {
                // Transition: Idle -> Speaking
                _state = SessionSpeechState.Speaking;
                _utteranceId++;
                _revision = 0;
                _silenceSamples = 0;
                _samplesSinceLastInterim = 0;

                // Copy pre-roll into utterance buffer
                var preRollArr = new float[_preRollBuffer.Count];
                int copied = _preRollBuffer.CopyTo(preRollArr);
                _utteranceBuffer.Append(preRollArr.AsSpan(0, copied));
                _preRollBuffer.Clear();
                Serilog.Log.Information("[STT] Speech detected! (prob: {Prob:F2}, peak: {Peak:F3}, rms: {Rms:F3}) Starting utterance #{UtteranceId}",
                    speechProbability, peak, rms, _utteranceId);
            }
        }
        else if (_state == SessionSpeechState.Speaking)
        {
            _utteranceBuffer.Append(frame.Span);
            _samplesSinceLastInterim += frame.Length;

            if (!isSpeechContinue)
            {
                _silenceSamples += frame.Length;
            }
            else
            {
                _silenceSamples = 0;
            }

            // Check endpointing: silence >= 1000ms or reached max duration
            int endSilenceThresholdSamples = _options.Vad.EndSilenceMilliseconds * 16;
            if (_silenceSamples >= endSilenceThresholdSamples ||
                _utteranceBuffer.Length >= _options.Utterance.MaximumSamples)
            {
                var finalSnapshot = _utteranceBuffer.Snapshot();
                long finishedUtteranceId = _utteranceId;

                // Reset buffer and state IMMEDIATELY so new audio is NOT dropped!
                _utteranceBuffer.Reset();
                _vadRunner.Reset();
                _state = SessionSpeechState.Idle;
                _consecutiveSpeechFrames = 0;
                _silenceSamples = 0;
                _samplesSinceLastInterim = 0;

                _ = FinalizeUtteranceSnapshotAsync(finishedUtteranceId, finalSnapshot, ct);
                return;
            }

            // Schedule interim transcription snapshot if cadence reached
            int interimSamplesCadence = _options.Interim.TargetIntervalMilliseconds * 16;
            if (_options.Interim.Enabled &&
                _samplesSinceLastInterim >= interimSamplesCadence &&
                (_inFlightInterimTask == null || _inFlightInterimTask.IsCompleted))
            {
                _samplesSinceLastInterim = 0;
                _revision++;
                var snapshot = _utteranceBuffer.Snapshot();
                _inFlightInterimTask = RunInterimInferenceAsync(_utteranceId, _revision, snapshot, ct);
            }
        }
    }

    private async Task RunInterimInferenceAsync(long utteranceId, long revision, float[] snapshot, CancellationToken ct)
    {
        try
        {
            if (snapshot.Length < 9600) return; // Skip if less than 600ms of audio to prevent hallucinations
            string text = await _transcriber.TranscribeAsync(snapshot, isFinal: false, ct);
            if (!string.IsNullOrWhiteSpace(text) && _state == SessionSpeechState.Speaking && _utteranceId == utteranceId)
            {
                await _channel.Writer.WriteAsync(new TranscriptEventDto(
                    SessionId, utteranceId, revision, text, false, DateTimeOffset.UtcNow), ct);
            }
        }
        catch (OperationCanceledException) { }
        catch
        {
            throw;
        }
    }

    private async Task FinalizeUtteranceSnapshotAsync(long utteranceId, float[] finalSnapshot, CancellationToken ct)
    {
        try
        {
            if (_inFlightInterimTask != null && !_inFlightInterimTask.IsCompleted)
            {
                await _inFlightInterimTask;
            }

            if (finalSnapshot.Length >= 9600) // At least 600ms
            {
                string finalResult = await _transcriber.TranscribeAsync(finalSnapshot, isFinal: true, ct);
                if (!string.IsNullOrWhiteSpace(finalResult))
                {
                    _revision++;
                    await _channel.Writer.WriteAsync(new TranscriptEventDto(
                        SessionId, utteranceId, _revision, finalResult, true, DateTimeOffset.UtcNow), ct);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch
        {
            throw;
        }
    }

    public IAsyncEnumerable<TranscriptEventDto> ReadTranscriptsAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }

    public async ValueTask CompleteInputAsync(CancellationToken cancellationToken = default)
    {
        if (_utteranceBuffer.Length >= 9600)
        {
            var snapshot = _utteranceBuffer.Snapshot();
            long finishedId = _utteranceId;
            _utteranceBuffer.Reset();
            _state = SessionSpeechState.Idle;
            await FinalizeUtteranceSnapshotAsync(finishedId, snapshot, cancellationToken);
        }
        _channel.Writer.Complete();
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();
        _decoder.Dispose();
        _vadRunner.Dispose();
        _processingLock.Dispose();
    }
}