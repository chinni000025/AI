import { inject, Injectable } from '@angular/core';
import { EngineCore } from './engine-core';
import { EngineConstants, EngineControllers, EngineRoutes } from './engine-route-constants';
import { TokenService } from './token-service';
import { Observable, tap, shareReplay, catchError, throwError } from 'rxjs';
import { RefreshTokenResponse } from '../models/snackbar-config';

@Injectable({
    providedIn: 'root',
})
export class IdentityService {
    private http = inject(EngineCore);
    private tokenService = inject(TokenService);

    // The global lock that shares the request
    private isRefreshing$: Observable<RefreshTokenResponse> | null = null;

    login(payload: any) {
        return this.http.post(`${EngineControllers.IdentitiyController}/user-login`, payload);
    }

    register(payload: any) {
        return this.http.post(`${EngineControllers.IdentitiyController}/user-register`, payload);
    }

    logout() {
        return this.http.post(`${EngineControllers.IdentitiyController}/user-logout`, {})
            .pipe(tap(() => {
                this.tokenService.clear();
            }));
    }

    refreshToken(): Observable<RefreshTokenResponse> {
        if (this.isRefreshing$) {
            return this.isRefreshing$;
        }

        this.isRefreshing$ = this.http.post<RefreshTokenResponse>(`${EngineControllers.IdentitiyController}/refresh-token`, {}).pipe(
            tap(() => {
                this.isRefreshing$ = null;
            }),
            catchError((error) => {
                this.isRefreshing$ = null;
                return throwError(() => error);
            }),
            shareReplay(1) // Emits the same result to all subscribers (Interceptor + SignalR)
        );

        return this.isRefreshing$;
    }

    forgetIdentity(payload: any) {
        return this.http.post(`${EngineControllers.IdentitiyController}/forget-identity`, payload);
    }

    resetIdentity(payload: any) {
        return this.http.post(`${EngineControllers.IdentitiyController}/reset-identity`, payload);
    }
}
