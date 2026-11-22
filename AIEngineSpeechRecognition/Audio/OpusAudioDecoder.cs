using Concentus.Structs;

namespace AIEngineSpeechRecognition.Audio
{
    public sealed class OpusAudioDecoder : IDisposable
    {
        private readonly OpusDecoder _stereoDecoder;
        private readonly OpusDecoder _monoDecoder;
        private readonly int _inputSampleRate;
        private readonly short[] _decodeBuffer;
        private int _decodeLogCount = 0;

        public OpusAudioDecoder(int inputSampleRate = 48000) : this(inputSampleRate, 2)
        {
        }

        public OpusAudioDecoder(int inputSampleRate, int channels)
        {
            _inputSampleRate = inputSampleRate;
            _stereoDecoder = new OpusDecoder(inputSampleRate, 2);
            _monoDecoder = new OpusDecoder(inputSampleRate, 1);
            // Max 120ms frame at 48kHz stereo (5760 samples per channel = 11520 shorts)
            _decodeBuffer = new short[inputSampleRate * 2 * 120 / 1000];
        }

        public int DecodeToFloat(ReadOnlySpan<byte> opusPayload, Span<float> destinationFloatPcm)
        {
            if (opusPayload.Length == 0) return 0;
            byte[] inBytes = opusPayload.ToArray();

            // RFC 6716 TOC byte bit 2 indicates stereo (1) or mono (0)
            bool isStereo = (inBytes[0] & 0x04) != 0;

            if (isStereo)
            {
                int samplesPerChannel = _stereoDecoder.Decode(
                    inBytes, 0, inBytes.Length,
                    _decodeBuffer, 0, _decodeBuffer.Length / 2, false);

                if (samplesPerChannel > 0)
                {
                    int samplesToWrite = Math.Min(samplesPerChannel, destinationFloatPcm.Length);
                    float maxAmp = 0f;
                    for (int i = 0; i < samplesToWrite; i++)
                    {
                        float left = _decodeBuffer[i * 2] / 32768.0f;
                        float right = _decodeBuffer[i * 2 + 1] / 32768.0f;
                        float val = (left + right) * 0.5f;
                        destinationFloatPcm[i] = val;
                        float abs = Math.Abs(val);
                        if (abs > maxAmp) maxAmp = abs;
                    }
                    return samplesToWrite;
                }
            }
            else
            {
                int monoSamples = _monoDecoder.Decode(inBytes, 0, inBytes.Length, _decodeBuffer, 0, _decodeBuffer.Length, false);

                if (monoSamples > 0)
                {
                    int samplesToWrite = Math.Min(monoSamples, destinationFloatPcm.Length);
                    float maxAmp = 0f;
                    for (int i = 0; i < samplesToWrite; i++)
                    {
                        float val = _decodeBuffer[i] / 32768.0f;
                        destinationFloatPcm[i] = val;
                        float abs = Math.Abs(val);
                        if (abs > maxAmp) maxAmp = abs;
                    }

                    return samplesToWrite;
                }
            }

            // Fallback: try stereo if mono failed or vice versa
            int fallbackSamples = _stereoDecoder.Decode(inBytes, 0, inBytes.Length, _decodeBuffer, 0, _decodeBuffer.Length / 2, false);

            if (fallbackSamples > 0)
            {
                int samplesToWrite = Math.Min(fallbackSamples, destinationFloatPcm.Length);
                for (int i = 0; i < samplesToWrite; i++)
                {
                    float left = _decodeBuffer[i * 2] / 32768.0f;
                    float right = _decodeBuffer[i * 2 + 1] / 32768.0f;
                    destinationFloatPcm[i] = (left + right) * 0.5f;
                }
                return samplesToWrite;
            }

            return 0;
        }

        public void Dispose()
        {
            _stereoDecoder.Dispose();
            _monoDecoder.Dispose();
        }
    }
}
