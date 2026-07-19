import { inject, Inject, Injectable } from '@angular/core';
import { EngineCore } from './services/engine-core';
import { EngineConstants, EngineControllers, EngineRoutes } from './services/engine-route-constants';

@Injectable({
  providedIn: 'root',
})
export class SettingsService {
  private http = inject(EngineCore);
  constructor() { }

  saveGoogleConnection(clientId: any, clientSecret: any): any {
    const endpoint = `${EngineControllers.ConnectionController}/saveGoogleConnection`
      + `?clientId=${encodeURIComponent(clientId)}`
      + `&clientSecret=${encodeURIComponent(clientSecret)}`;
    return this.http.post(endpoint, null);
  }

  sendTestEmail(payload: any): any {
    return this.http.post(`${EngineControllers.ConnectionController}/testMail`, payload);
  }

  saveSmtpSettings(payload: any) {
    return this.http.post(`${EngineControllers.ConnectionController}/savesmtpConfiguration`, payload);
  }
}
