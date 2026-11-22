namespace AIEngineSpeechRecognition.Audio
{
    public sealed class StreamingAudioResampler
    {
        public readonly int _sourceRate;
        public readonly int _targetRate;
        public readonly int _ratio;
        public StreamingAudioResampler(int sourceRate = 48000, int targetRate = 16000)
        {
            _sourceRate = sourceRate;
            _targetRate = targetRate;
            _ratio = sourceRate / targetRate;
        }

        public int Resample3To1(ReadOnlySpan<float> input48k, Span<float> output16k)
        {
            int outputCount = input48k.Length / _ratio;
            for (int i = 0; i < outputCount; i++)
            {
                int srcIdx = i * _ratio;
                // 3-point anti-aliasing box filter (or sinc convolution)
                output16k[i] = (input48k[srcIdx] + input48k[srcIdx + 1] + input48k[srcIdx + 2]) / 3.0f;
            }
            return outputCount;
        }
    }
}