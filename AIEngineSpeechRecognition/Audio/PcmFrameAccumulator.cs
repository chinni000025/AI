namespace AIEngineSpeechRecognition.Audio;

public sealed class PcmFrameAccumulator
{
    private readonly float[] _buffer;
    private int _count;

    public int FrameSize => _buffer.Length;

    public PcmFrameAccumulator(int frameSize = 512)
    {
        _buffer = new float[frameSize];
    }

    public void Append(ReadOnlySpan<float> samples, Action<ReadOnlyMemory<float>> onFrameReady)
    {
        int offset = 0;
        while (offset < samples.Length)
        {
            int toCopy = Math.Min(_buffer.Length - _count, samples.Length - offset);
            samples.Slice(offset, toCopy).CopyTo(_buffer.AsSpan(_count));
            _count += toCopy;
            offset += toCopy;

            if (_count == _buffer.Length)
            {
                var frameCopy = new float[_buffer.Length];
                Array.Copy(_buffer, frameCopy, _buffer.Length);
                onFrameReady(frameCopy);
                _count = 0;
            }
        }
    }

    public void Reset() => _count = 0;
}