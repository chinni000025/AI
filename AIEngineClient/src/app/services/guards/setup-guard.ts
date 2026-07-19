import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { EngineService } from '../engine-service';
import { EngineStatus } from '../engine-status';

export const setupGuard: CanActivateFn = () => {

	const engineStatus = inject(EngineStatus);
	const router = inject(Router);
	if (!engineStatus.isEngineConfigured()) {
		return true;
	}
	router.navigate(['']);
	return false;
};
