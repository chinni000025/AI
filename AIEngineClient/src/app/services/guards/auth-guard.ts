import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../token-service';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = () => {
    const tokenService = inject(TokenService);
    if (tokenService.isLoggedIn()) {
        return true;
    }

    const router = inject(Router);
    router.navigate(['/']);
    return false;
};
