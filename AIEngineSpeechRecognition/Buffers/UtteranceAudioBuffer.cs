namespace AIEngineSpeechRecognition.Buffers;

public sealed class UtteranceAudioBuffer
{
    private readonly float[] _buffer;
    private int _length;

    public int Length => _length;
    public int Capacity => _buffer.Length;

    public UtteranceAudioBuffer(int maxSamples = 400000) // 25s at 16kHz
    {
        _buffer = new float[maxSamples];
    }

    public bool Append(ReadOnlySpan<float> samples)
    {
        int space = _buffer.Length - _length;
        int toCopy = Math.Min(space, samples.Length);
        samples.Slice(0, toCopy).CopyTo(_buffer.AsSpan(_length));
        _length += toCopy;
        return _length < _buffer.Length;
    }

    public float[] Snapshot()
    {
        var copy = new float[_length];
        Array.Copy(_buffer, copy, _length);
        return copy;
    }

    public void Reset() => _length = 0;
}