import { Injectable } from "@angular/core";
import { SttTranscripState as SttTranscripState, SttTranscriptEvent } from "../../models/stt.models";
import { BehaviorSubject } from "rxjs";
import { SnackbarService } from "../snackbar-service";

@Injectable({
    providedIn: 'root'
})
export class SttTranscriptStore {
    constructor(private snackBarService: SnackbarService) { }
    private readonly initialState: SttTranscripState = {
        sessionId: null,
        currentUtteranceId: null,
        interimText: '',
        finalizedUtterances: []
    }

    private readonly stateSubject = new BehaviorSubject<SttTranscripState>(this.initialState);
    readonly state$ = this.stateSubject.asObservable();
    private latestUtteranceId = -1;
    private readonly latestRevisionByUtterance = new Map<number, number>();
    private readonly finalizedUtteranceIds = new Set<number>();


    //Starts a new transcript session and clears previous transcript data.
    startSession(sessionId: string): void {
        if (!sessionId?.trim()) {
            this.snackBarService.showErrorMessage("A valid STT session ID is required.");
        }

        this.latestUtteranceId = -1;
        this.latestRevisionByUtterance.clear();
        this.finalizedUtteranceIds.clear();

        this.stateSubject.next({
            sessionId,
            currentUtteranceId: null,
            interimText: '',
            finalizedUtterances: []
        });
    }


    // Processes a complete transcript snapshot from the backend.
    //Returns false when the event is stale, duplicated,
    // belongs to another session, or arrives after finalization.

    applyTranscript(event: SttTranscriptEvent): boolean {
        const currentState = this.stateSubject.value;

        if (!currentState.sessionId || event.sessionId !== currentState.sessionId) {
            return false;
        }

        if (!Number.isInteger(event.utteranceId) || !Number.isInteger(event.revision) ||
            event.utteranceId < 0 || event.revision < 0) {
            return false;
        }

        if (event.utteranceId < this.latestUtteranceId) {
            return false;
        }

        if (this.finalizedUtteranceIds.has(event.utteranceId)) {
            return false;
        }

        const previousRevision = this.latestRevisionByUtterance.get(event.utteranceId);

        if (previousRevision !== undefined && event.revision <= previousRevision) {
            return false;
        }

        this.latestRevisionByUtterance.set(event.utteranceId, event.revision);
        this.latestUtteranceId = Math.max(this.latestUtteranceId, event.utteranceId);

        if (event.isFinal) {
            this.finalizedUtteranceIds.add(event.utteranceId);

            this.stateSubject.next({
                ...currentState,
                currentUtteranceId: event.utteranceId,
                interimText: '',
                finalizedUtterances: [
                    ...currentState.finalizedUtterances,
                    event.text
                ]
            });

            return true;
        }

        this.stateSubject.next({
            ...currentState,
            currentUtteranceId: event.utteranceId,
            interimText: event.text
        });

        return true;
    }


    //Clears transcript data and ends the current local session.
    reset(): void {
        this.latestUtteranceId = -1;
        this.latestRevisionByUtterance.clear();
        this.finalizedUtteranceIds.clear();

        this.stateSubject.next(this.initialState);
    }

}