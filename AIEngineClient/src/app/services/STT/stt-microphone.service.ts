
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
            this.snackbar.showErrorMessage("Microphone access is not supported by this browser or environment.");
        }

        const stream = await navigator.mediaDevices.getUserMedia({
            audio: {
                echoCancellation: true, //Echo noise
                noiseSuppression: true, //Background noise.
                autoGainControl: true //Auto mic gain(volume) control.
            },
            video: false
        });

        const audioTracks = stream.getAudioTracks();

        if (audioTracks.length === 0) {
            stream.getTracks().forEach(track => track.stop());
            this.snackbar.showErrorMessage("No microphone audio track was provided by the browser.");
        }

        this.activeStream = stream;

        return stream;
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
