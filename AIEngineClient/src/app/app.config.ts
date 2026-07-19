import { APP_INITIALIZER, ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { EngineStatus } from './services/engine-status';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi, withXsrfConfiguration } from '@angular/common/http';
import { AuthInterceptor } from './services/interceptors/auth-interceptor';
import { RefreshTokenInterceptor } from './services/interceptors/refresh-token-interceptor';
import { provideMarkdown } from 'ngx-markdown';
import { EncryptionInterceptor } from './services/interceptors/encryption-interceptor';

export const appConfig: ApplicationConfig = {
    providers: [
        provideBrowserGlobalErrorListeners(),
        provideRouter(routes),
        provideHttpClient(withInterceptorsFromDi()),
        {
            provide: APP_INITIALIZER,
            useFactory: (status: EngineStatus) => () => status.loadEngineStatus(),
            deps: [EngineStatus],
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: RefreshTokenInterceptor,
            multi: true
        },
        {
			provide: HTTP_INTERCEPTORS,
			useClass: EncryptionInterceptor,
			multi: true
		},
        provideMarkdown()
    ]
};
