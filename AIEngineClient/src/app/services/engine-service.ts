import { booleanAttribute, inject, Injectable } from '@angular/core';
import { EngineCore } from './engine-core';
import { environment } from '../../environments/environment.prod';
import { EngineControllers, EngineRoutes } from './engine-route-constants';

@Injectable({
    providedIn: 'root',
})
export class EngineService {
    private http = inject(EngineCore);
    constructor() { }
    configuredEngineDataBase(payload: {}) {
        return this.http.post(`${EngineControllers.EngineStatusController}/configure-database`, payload);
    }

    getEngineStatus() {
        return this.http.get<{ isDataBaseConfigure: Boolean }>
            (`${EngineControllers.EngineStatusController}/engine-status`);
    }

    testConnection(payload: {}) {
        return this.http.post(`${EngineControllers.EngineStatusController}/test-engine`, payload);
    }

    getEngineState() {
        return this.http.get<{
            isEngineRunning: boolean;
            isEngineReady: boolean;
            errorMessage: any;
        }>(`${EngineControllers.EngineStateController}/engine-status`);
    }
}
