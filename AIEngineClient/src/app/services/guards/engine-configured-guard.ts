import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { EngineStatus } from '../engine-status';
import { EngineRoutes } from '../engine-route-constants';
import { TokenService } from '../token-service';

export const engineConfiguredGuard: CanActivateFn = async () => {
    const engineService = inject(EngineStatus);
    const tokenService = inject(TokenService);
    const router = inject(Router);

    await engineService.loadEngineStatus();

    if (!engineService.isEngineConfigured()) {
        return router.createUrlTree([EngineRoutes.EngineSetup]);
    }
    if (!engineService.isEngineReady()) {
        return router.createUrlTree([EngineRoutes.Processing]);
    }

    if (tokenService.isLoggedIn()) {
        return router.createUrlTree([EngineRoutes.PromptSpace]);
    }

    return true;
};
