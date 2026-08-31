import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { EncryptionService } from '../encryption.service';
import { ExcludeEncryptionEndPoints } from '../engine-route-constants';

@Injectable()
export class EncryptionInterceptor implements HttpInterceptor {

    constructor(private encryptionService: EncryptionService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (!this.shouldEncrypt(request)) {
            return next.handle(request);
        }

        return from(this.encryptionService.encrypt(request.body)).pipe(
            switchMap(encrypted => {
                const encryptedRequest = request.clone({
                    body: {
                        payload: encrypted
                    },
                    setHeaders: {
                        'AIEngine-Encryption': 'true'
                    }
                });
                return next.handle(encryptedRequest);
            })

        );
    }

    private shouldEncrypt(request: HttpRequest<any>): boolean {
        if (request.method === 'GET')
            return false;

        if (!request.body || request.body instanceof FormData)
            return false;
        if (this.isExcludedEndPoints(request)) {
            return false;
        }

        const contentType = request.headers.get('Content-Type');

        if (contentType?.includes('multipart/form-data'))
            return false;

        return true;
    }

    private isExcludedEndPoints(request: HttpRequest<any>): boolean {
        return ExcludeEncryptionEndPoints.some(e => request.url.includes(e));
    }

}