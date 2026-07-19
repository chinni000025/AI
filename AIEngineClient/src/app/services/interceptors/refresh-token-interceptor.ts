import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { BehaviorSubject, catchError, filter, Observable, switchMap, take, throwError } from "rxjs";
import { EngineControllers } from "../engine-route-constants";
import { IdentityService } from "../identity-service";
import { TokenService } from "../token-service";
import { Router } from "@angular/router";
import { Injectable } from "@angular/core";

@Injectable()
export class RefreshTokenInterceptor implements HttpInterceptor {
    private isRefreshing = false; // Prevent Race Conditions.
    private isRefeshSubject = new BehaviorSubject<string | null>(null);
    constructor(private ids: IdentityService, private tokenService: TokenService,
        private router: Router) { }

    intercept(req: HttpRequest<any>, next: HttpHandler) {
        if (req.url.includes(`${EngineControllers.IdentitiyController}/user-login`)
            || req.url.includes(`${EngineControllers.IdentitiyController}/refresh-token`)) {
            return next.handle(req);
        }

        return next.handle(req).pipe(
            catchError(err => {
                if (err.status === 401) {
                    return this.handle401(req, next);
                }
                return throwError(() => err);
            })
        );
    }

    private handle401(req: HttpRequest<any>, next: HttpHandler) {
        if (!this.isRefreshing) {
            this.isRefreshing = true;
            this.isRefeshSubject.next(null);

            return this.ids.refreshToken().pipe(
                switchMap(newToken => {
                    this.tokenService.setAccessToken(newToken.response.engineIgnition);
                    this.tokenService.setEngineValidationToken(newToken.response.engineValidation);
                    this.isRefeshSubject.next(newToken.response.engineIgnition);
                    this.isRefreshing = false;

                    return next.handle(
                        req.clone({
                            setHeaders: {
                                Authorization: `Bearer ${newToken.response.engineIgnition}`
                            }
                        })
                    );
                }),

                catchError(err => {
                    console.log(err);
                    this.isRefreshing = false;
                    this.tokenService.clear();
                    this.router.navigate(['/']);
                    return throwError(() => err);
                })

            );
        }

        return this.isRefeshSubject.pipe(
            filter(token => token != null), take(1),
            switchMap(token => {
                return next.handle(req.clone({
                    setHeaders: {
                        Authorization: `Bearer ${token}`
                    }
                }))
            })
        )
    }
}
