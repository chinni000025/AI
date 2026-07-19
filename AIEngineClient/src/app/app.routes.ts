import { Routes } from '@angular/router';
import { PromptSpace } from './Components/prompt-space/prompt-space';
import { Identity } from './Components/identity/identity';
import { EngineSetup } from './Components/engine-setup/engine-setup';
import { ForgotIdentity } from './Components/forgot-identity/forgot-identity';
import { ResetIdentity } from './Components/reset-identity/reset-identity';
import { ProcessingView } from './Components/processing-view/processing-view';
import { EngineConstants, EngineRoutes } from './services/engine-route-constants';
import { engineConfiguredGuard } from './services/guards/engine-configured-guard';
import { authGuard } from './services/guards/auth-guard';
import { setupGuard } from './services/guards/setup-guard';

export const routes: Routes = [
    { path: '', component: Identity, canActivate: [engineConfiguredGuard] },
    { path: EngineRoutes.PromptSpace, component: PromptSpace, canActivate: [authGuard] },
    { path: `${EngineRoutes.PromptSpace}/:${EngineConstants.ConversationId}`, component: PromptSpace, canActivate: [authGuard] },
    { path: EngineRoutes.EngineSetup, component: EngineSetup, canActivate: [setupGuard] },
    { path: EngineRoutes.ForgetIdentity, component: ForgotIdentity },
    { path: EngineRoutes.ResetIdentity, component: ResetIdentity },
    { path: EngineRoutes.Processing, component: ProcessingView },
    { path: '**', redirectTo: '' },
];