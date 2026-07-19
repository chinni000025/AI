import { HTTP_INTERCEPTORS, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpXsrfTokenExtractor } from "@angular/common/http";
import { Observable } from "rxjs";
import { EngineConstants, EngineControllers } from "../engine-route-constants";
import { TokenService } from "../token-service";
import { Injectable } from "@angular/core";
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private tokenService: TokenService) { }
    intercept(req: HttpRequest<any>, next: HttpHandler) {
        let headers = req.headers;

        if (!req.url.includes(`${EngineControllers.IdentitiyController}/user-login`)
            && !req.url.includes(`${EngineControllers.IdentitiyController}/refresh-token`)
            && !req.url.includes(`${EngineControllers.IdentitiyController}/user-logout`)) {
            const token = this.tokenService.getAccessToken();
            if (token) {
                headers = headers.set('Authorization', `Bearer ${token}`);
            }
        }

        if (!req.url.includes(`${EngineControllers.IdentitiyController}/user-login`)) {
            const token = this.tokenService.getEngineValidationToken();
            if (token) {
                headers = headers.set(EngineConstants.EngineVerification, token);
            }
        }

        let isRequestBodyExists = req.body !== null && req.body !== undefined;
        let isFormData = req.body instanceof FormData;
        if (req.method !== 'GET' && isRequestBodyExists && !isFormData) {
            headers = headers.set('Content-Type', 'application/json');
        }

        const auth = req.clone({
            headers,
            withCredentials: true
        });
        return next.handle(auth);
    }
}
