export type SttSessionState = | 'idle' | 'requesting-permission'
    | 'connecting' | 'listening' | 'reconnecting' | 'stopping' | 'error';

export interface SttTranscriptEvent {
    sessionId: string;
    utteranceId: number;
    revision: number;
    text: string;
    isFinal: boolean;
    createdAt: string;
}

export interface SttError {
    code: string;
    message: string;
    recoverable: boolean;
}

export interface SttTranscripState {
    sessionId: string | null,
    currentUtteranceId: number | null,
    interimText: string,
    finalizedUtterances: string[]
}