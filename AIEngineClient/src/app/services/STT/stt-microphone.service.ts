
import { Injectable } from '@angular/core';
import { SnackbarService } from '../snackbar-service';

@Injectable({
    providedIn: 'root'
})
export class SttMicrophoneService {
    constructor(private snackbar: SnackbarService) { }
    private activeStream: MediaStream | null = null;

    get stream(): MediaStream | null {
        return this.activeStream;
    }

    get isActive(): boolean {
        //Checks whether the track is live or ended.
        return !!this.activeStream &&
            this.activeStream.getAudioTracks().some(
                track => track.readyState === 'live'
            );
    }


    //  Requests microphone access and returns the active audio stream.
    async acquire(): Promise<MediaStream> {
        if (this.isActive) {
            return this.activeStream!;
        }
        if (!navigator.mediaDevices?.getUserMedia) {
            const error = new Error('Microphone access is not supported by this browser or environment.');
            this.snackbar.showErrorMessage(error.message);
            throw error;
        }

        let stream: MediaStream | null = null;

        try {
            stream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    echoCancellation: true,
                    noiseSuppression: true,
                    autoGainControl: true
                },
                video: false
            });

            const audioTracks = stream.getAudioTracks();

            if (audioTracks.length === 0 || !audioTracks.some(track => track.readyState === 'live')) {
                throw new Error('No active microphone audio track was provided by the browser.');
            }

            this.activeStream = stream;
            return stream;

        } catch (error) {
            if (stream) {
                stream.getTracks().forEach(track => track.stop());
            }
            this.activeStream = null;
            throw error;
        }
    }

    //Release microphone resources.

    release(): void {
        if (!this.activeStream) {
            return;
        }

        this.activeStream.getTracks().forEach(track => {
            track.stop();
        });

        this.activeStream = null;
    }
}
