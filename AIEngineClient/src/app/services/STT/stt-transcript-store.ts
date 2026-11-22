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
    private readonly finalizedTextByUtterance = new Map<number, string>();

    //Starts a new transcript session and clears previous transcript data.
    startSession(sessionId: string): void {
        if (!sessionId?.trim()) {
            this.snackBarService.showErrorMessage('A valid STT session ID is required.');
            return;
        }

        this.latestUtteranceId = -1;
        this.latestRevisionByUtterance.clear();
        this.finalizedUtteranceIds.clear();
        this.finalizedTextByUtterance.clear();

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

    applyTranscript(rawEvent: any): boolean {
        if (!rawEvent) return false;

        const rawText = (rawEvent.text ?? rawEvent.Text ?? '').toString();
        const cleanText = rawText.replace(/\[.*?\]|\(.*?\)/g, '').trim();

        const event: SttTranscriptEvent = {
            sessionId: (rawEvent.sessionId ?? rawEvent.SessionId ?? '').toString().toLowerCase(),
            utteranceId: Number(rawEvent.utteranceId ?? rawEvent.UtteranceId ?? 0),
            revision: Number(rawEvent.revision ?? rawEvent.Revision ?? 0),
            text: cleanText,
            isFinal: Boolean(rawEvent.isFinal ?? rawEvent.IsFinal ?? false),
            createdAt: (rawEvent.createdAt ?? rawEvent.CreatedAt ?? new Date().toISOString()).toString()
        };

        if (!cleanText && !event.isFinal) {
            return false;
        }

        const currentState = this.stateSubject.value;
        const currentSessionId = (currentState.sessionId ?? '').toLowerCase().replace(/[{}]/g, '');
        const eventSessionId = (event.sessionId ?? '').toLowerCase().replace(/[{}]/g, '');

        if (currentSessionId && eventSessionId && eventSessionId !== currentSessionId) {
            return false;
        }

        // If store didn't have sessionId yet, attach it from first event
        if (!currentState.sessionId && eventSessionId) {
            currentState.sessionId = eventSessionId;
        }

        if (!Number.isInteger(event.utteranceId) || !Number.isInteger(event.revision) ||
            event.utteranceId < 0 || event.revision < 0 ||
            typeof event.text !== 'string' || typeof event.isFinal !== 'boolean') {
            return false;
        }

        if (this.finalizedUtteranceIds.has(event.utteranceId)) {
            return false;
        }
        const previousRevision = this.latestRevisionByUtterance.get(event.utteranceId);

        if (previousRevision !== undefined && event.revision <= previousRevision) {
            return false;
        }

        // Ignore stale interim events, but allow legitimate
        // delayed final events for older utterances.
        if (event.utteranceId < this.latestUtteranceId && !event.isFinal) {
            return false;
        }

        this.latestRevisionByUtterance.set(
            event.utteranceId,
            event.revision
        );

        this.latestUtteranceId = Math.max(
            this.latestUtteranceId,
            event.utteranceId
        );

        if (event.isFinal) {
            this.finalizedUtteranceIds.add(event.utteranceId);

            this.finalizedTextByUtterance.set(
                event.utteranceId,
                event.text
            );

            const finalizedUtterances = Array.from(
                this.finalizedTextByUtterance.entries()
            )
                .sort(([idA], [idB]) => idA - idB)
                .map(([, text]) => text);

            const isCurrentUtterance =
                currentState.currentUtteranceId === event.utteranceId;

            this.stateSubject.next({
                ...currentState,
                currentUtteranceId: isCurrentUtterance
                    ? null
                    : currentState.currentUtteranceId,
                interimText: isCurrentUtterance
                    ? ''
                    : currentState.interimText,
                finalizedUtterances
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
        this.finalizedTextByUtterance.clear();

        this.stateSubject.next(this.initialState);
    }
}