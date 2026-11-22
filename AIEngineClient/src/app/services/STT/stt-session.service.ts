
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import {
    SttError,
    SttSessionState
} from '../../models/stt.models';
import { SnackbarService } from '../snackbar-service';

export interface SttSessionUiState {
    state: SttSessionState;
    error: SttError | null;
}

@Injectable({
    providedIn: 'root'
})
export class SttSessionService {
    constructor(private snackbarService: SnackbarService) { }
    private readonly initialState: SttSessionUiState = {
        state: 'idle',
        error: null
    };

    private readonly stateSubject = new BehaviorSubject<SttSessionUiState>(this.initialState);

    readonly state$ = this.stateSubject.asObservable();

    get snapshot(): SttSessionUiState {
        return this.stateSubject.value;
    }


    //Begins a new UI session for the selected composer.
    begin(): boolean {
        const currentState = this.snapshot.state;

        if (currentState !== 'idle' && currentState !== 'error') {
            this.snackbarService.showErrorMessage(`Cannot start an STT session while the current state is '${currentState}'.`);
            return false;
        }

        this.updateState({
            state: 'requesting-permission',
            error: null
        });
        return true;
    }

    /**
     * Called after microphone permission is granted and
     * the frontend begins establishing the connection.
     */
    markConnecting(): void {
        this.transitionTo('connecting', [
            'requesting-permission',
            'reconnecting'
        ]);
    }

    /**
     * Called when the audio connection is ready.
     */
    markListening(): void {
        this.transitionTo('listening', [
            'connecting',
            'reconnecting'
        ]);
    }

    /**
     * Called when the active connection is interrupted
     * and reconnection is being attempted.
     */
    markReconnecting(): void {
        this.transitionTo('reconnecting', [
            'connecting',
            'listening'
        ]);
    }

    /**
     * Called when the user or application starts stopping
     * the active STT session.
     */
    markStopping(): void {
        this.transitionTo('stopping', [
            'requesting-permission',
            'connecting',
            'listening',
            'reconnecting'
        ]);
    }

    /**
     * Records an STT error and moves the UI into the error state.
     */
    fail(error: SttError): void {
        if (this.snapshot.state === 'idle') {
            return;
        }

        this.updateState({
            ...this.snapshot,
            state: 'error',
            error
        });
    }

    /**
     * Resets the UI state after session cleanup is complete.
     */
    reset(): void {
        this.updateState(this.initialState);
    }

    private transitionTo(
        nextState: SttSessionState,
        allowedCurrentStates: SttSessionState[]
    ): boolean {
        const currentState = this.snapshot;

        if (!allowedCurrentStates.includes(currentState.state)) {
            this.snackbarService.showErrorMessage(`Invalid STT state transition: '${currentState.state}' to '${nextState}'.`);

            return false;
        }

        this.updateState({
            ...currentState,
            state: nextState,
            error: null
        });

        return true;
    }

    private updateState(state: SttSessionUiState): void {
        this.stateSubject.next(state);
    }
}