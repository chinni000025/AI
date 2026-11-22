flowchart TD
    A["Browser Mic - 48kHz"]
    -->|Opus RTP Packets via WebRTC| B["AIEngineGateway"]

    B --> C["IStreamingSttSession.PushEncodedAudioPacketAsync"]

    C --> D["OpusAudioDecoder"]
    D -->|48kHz Float32 PCM| E["StreamingAudioResampler"]

    E -->|Downsample 3:1| F["16kHz Mono Float32"]

    F --> G["PcmFrameAccumulator"]
    G -->|512 Samples / 32ms| H["SileroOnnxVadRunner"]

    G -.-> I["FloatRingBuffer<br/>320ms Pre-roll During Idle"]

    H --> J{"Speech Probability P"}

    J -->|P >= 0.55 for 2 Frames| K["Speech Start"]
    I -->|Flush Pre-roll| K

    K --> L["State: Speaking"]
    L --> M["UtteranceAudioBuffer"]

    M --> N{"600ms Cadence"}

    N -->|Yes| O["Snapshot Audio Buffer"]
    O --> P["Whisper.NET Interim Inference"]

    P --> Q["SignalR: TranscriptReceived<br/>isFinal = false"]
    Q --> R["Angular UI: Render Live Interim Transcript"]

    M --> S{"Silence P < 0.45<br/>for 800ms"}

    S -->|Yes| T["State: Finalizing"]

    T --> U["Whisper.NET<br/>Authoritative Final Inference"]

    U --> V["SignalR: TranscriptReceived<br/>isFinal = true"]

    V --> W["Angular UI: Lock Finalized Text"]

    S -->|No| M
    N -->|No| M