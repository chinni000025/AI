import { Injectable } from '@angular/core';
import { EngineConstants } from './engine-route-constants';

@Injectable({
    providedIn: 'root',
})
export class TokenService {
    private readonly _accessToken = EngineConstants.AccessToken;
    private readonly _sessionId = EngineConstants.SessionId;
    private readonly _engineValidationToken = EngineConstants.EngineValidationToken;
    private readonly _uploadSessionId = EngineConstants.UploadingSessionId

    getAccessToken(): any {
        return localStorage.getItem(this._accessToken);
    }

    setAccessToken(accessToken: any) {
        localStorage.setItem(this._accessToken, accessToken); // Key , value.
    }

    setEngineValidationToken(engineValiationToken: any) {
        localStorage.setItem(this._engineValidationToken, engineValiationToken);//Engine Validation Token.
    }

    setUploadSessionId(uploadSessionId: any) {
        localStorage.setItem(this._uploadSessionId, uploadSessionId);
    }

    getUploadSessionId(): any {
        return localStorage.getItem(this._uploadSessionId);
    }

    getEngineValidationToken(): any {
        return localStorage.getItem(this._engineValidationToken);
    }

    setSessionId(sessionId: any) {
        localStorage.setItem(this._sessionId, sessionId);
    }

    getSessionId(): string | null {
        return localStorage.getItem(this._sessionId);
    }

    ensureSessionId(): string {
        let sessionId = this.getSessionId();
        if (!sessionId) {
            sessionId = crypto.randomUUID(); //UUID (universal unique identifier).
            this.setSessionId(sessionId);
        }
        return sessionId;
    }

    clear() {
        localStorage.removeItem(this._accessToken);
        localStorage.removeItem(this._sessionId);
        localStorage.removeItem(this._engineValidationToken);
        localStorage.removeItem(this._uploadSessionId);
    }

    isLoggedIn(): boolean {
        return !!this.getAccessToken();
    }
}
