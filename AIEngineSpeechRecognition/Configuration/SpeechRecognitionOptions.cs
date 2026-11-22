namespace AIEngineSpeechRecognition.Configuration
{
    public class SpeechRecognitionOptions
    {
        public const string SectionName = "SpeechRecognition";
        public bool Enabled { get; set; } = true;
        public int InputSampleRate { get; set; } = 48000;
        public int TargetSampleRate { get; set; } = 16000;
        public int Channels { get; set; } = 1;
        public VadOptions Vad { get; set; } = new();
        public UtteranceOptions Utterance { get; set; } = new();
        public InterimOptions Interim { get; set; } = new();
        public InferenceOptions Inference { get; set; } = new();
    }

    public class VadOptions
    {
        public string ModelPath { get; set; } = "Models/silero_vad.onnx";
        public float SpeechStartThreshold { get; set; } = 0.15f;
        public float SpeechContinueThreshold { get; set; } = 0.08f;
        public int StartConfirmationFrames { get; set; } = 2;    // 2 consecutive frames (~64ms) to confirm speech
        public int EndSilenceMilliseconds { get; set; } = 1000;  // 1.0s of real silence to finalize, avoiding cutoffs
        public int PreRollMilliseconds { get; set; } = 320;
        public int MinimumSpeechMilliseconds { get; set; } = 400;
        public int FrameSize { get; set; } = 512; // 32ms at 16kHz
    }

    public class UtteranceOptions
    {
        public int MaximumDurationSeconds { get; set; } = 25; // Continuous speech can last up to 25s
        public int MaximumSamples => MaximumDurationSeconds * 16000; // 400,000 samples
    }

    public class InterimOptions
    {
        public bool Enabled { get; set; } = true;
        public int MinimumNewAudioMilliseconds { get; set; } = 250;
        public int TargetIntervalMilliseconds { get; set; } = 350; // Emit interim updates frequently
    }

    public class InferenceOptions
    {
        public int MaximumConcurrentOperations { get; set; } = 2;
        public int QueueCapacity { get; set; } = 8;
        public string ModelPath { get; set; } = "Models/ggml-base.en.bin";
    }
}
