import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { DialogService } from '../dialog.service';
import { catchError, Observable, throwError } from 'rxjs';
import { EngineErrorResponseDto } from '../engine-route-constants';

@Injectable()
export class httpErrorInterceptor implements HttpInterceptor {
  private isCriticalDialogOpen = false;

  constructor(
    private dialogService: DialogService
  ) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        // Skip 499 (client cancelled/aborted) and 401 (handled by RefreshTokenInterceptor)
        if (error.status === 499 || error.status === 401) {
          return throwError(() => error);
        }

        let correlationId = error.headers?.get('X-Correlation-ID') || undefined;
        let userMessage = 'An unexpected error occurred while processing your request.';

        if (error.error && typeof error.error === 'object') {
          const body = error.error as EngineErrorResponseDto;
          if (body.errorMessage) {
            userMessage = body.errorMessage;
          }
          if (body.correlationId) {
            correlationId = body.correlationId;
          }
        } else if (typeof error.error === 'string') {
          try {
            const parse = JSON.parse(error.error) as EngineErrorResponseDto;
            if (parse.errorMessage) userMessage = parse.errorMessage;
            if (parse.correlationId) correlationId = parse.correlationId;
          } catch {
            if (error.error.length > 0 && error.error.length < 200) {
              userMessage = error.error;
            }
          }
        }

        // Always show the dialog for any exception from the backend or connection drop
        if (!this.isCriticalDialogOpen) {
          this.isCriticalDialogOpen = true;
          const dialogRef = this.dialogService.openCriticalError({
            title: error.status === 0 ? 'Connection Error' : 'System Exception',
            message: error.status === 0
              ? 'Unable to communicate with the backend server. Please verify your connection or server status.'
              : userMessage,
            correlationId: correlationId,
            statusCode: error.status,
            okText: 'OK',
            cancelText: 'Cancel'
          });

          dialogRef.afterClosed().subscribe(() => {
            this.isCriticalDialogOpen = false;
          });
        }

        return throwError(() => error);
      })
    );
  }
}