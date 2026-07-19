import { inject, Injectable } from '@angular/core';
import { EngineCore } from './engine-core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment.prod';
import { EngineControllers, HubEndpoints } from './engine-route-constants';
import { SnackbarService } from './snackbar-service';
import { EngineService } from './engine-service';
import { SignalRService } from './signalr-service';

@Injectable({
    providedIn: 'root',
})
export class EngineStatus {
    private isConfigure = false;
    private snackService = inject(SnackbarService);
    private engineService = inject(EngineService);
    private EngineReady = false;
    private signalR = inject(SignalRService);
    async loadEngineStatus(): Promise<void> {
        try {
            this.signalR.startConnection({ hubName: HubEndpoints.EngineStatusHub, requireAuthentication: false });
            const dbstatus = await firstValueFrom(this.engineService.getEngineStatus());
            this.isConfigure = !!dbstatus.isDataBaseConfigure;
            if (this.isConfigure) {
                const engineState = await firstValueFrom(this.engineService.getEngineState());
                this.EngineReady = !!engineState.isEngineReady;
            }
        } catch (err) {
            this.snackService.showErrorMessage("DataBase is Not Configured")
            this.isConfigure = false;
            this.EngineReady = false;
        }
    }

    isEngineConfigured(): boolean {
        return this.isConfigure;
    }

    isEngineReady(): boolean {
        return this.EngineReady;
    }
}
