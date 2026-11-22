namespace AIEngineSpeechRecognition.Buffers;

public sealed class FloatRingBuffer
{
    private readonly float[] _buffer;
    private int _writeIndex;
    private int _count;

    public int Count => _count;

    public FloatRingBuffer(int capacity = 5120) // 320ms at 16kHz
    {
        _buffer = new float[capacity];
    }

    public void Write(ReadOnlySpan<float> samples)
    {
        foreach (var sample in samples)
        {
            _buffer[_writeIndex] = sample;
            _writeIndex = (_writeIndex + 1) % _buffer.Length;
            _count = Math.Min(_count + 1, _buffer.Length);
        }
    }

    public int CopyTo(Span<float> destination)
    {
        int available = Math.Min(_count, destination.Length);
        int start = (_writeIndex - _count + _buffer.Length) % _buffer.Length;
        for (int i = 0; i < available; i++)
        {
            destination[i] = _buffer[(start + i) % _buffer.Length];
        }
        return available;
    }

    public void Clear()
    {
        _writeIndex = 0;
        _count = 0;
    }
}