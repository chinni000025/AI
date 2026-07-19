import { Injectable } from '@angular/core';
import * as SignalR from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { TokenService } from './token-service';
import { environment } from '../../environments/environment.developement';
import { IdentityService } from './identity-service';
import { RefreshTokenResponse } from '../models/snackbar-config';
import { HubConfiguration } from './engine-route-constants';

@Injectable({
    providedIn: 'root',
})
export class SignalRService {
    private hubConnection = new Map<string, SignalR.HubConnection>(); // TCP or Websocket connection with multiple Hub Supports.
    private connectionStates = new Map<String, BehaviorSubject<boolean>>(); // Hub Connection state.
    private eventMap = new Map<string, Map<string, Subject<any>>>(); // Hub Name , Event Name, Subject.
    private savedOptions = new Map<string, HubConfiguration>();
    constructor(private tokenservice: TokenService, private ids: IdentityService) { }

    startConnection(hubConfiguration: HubConfiguration) {
        const exisitingConnection = this.hubConnection.get(hubConfiguration.hubName);
        if (exisitingConnection && exisitingConnection.state !== SignalR.HubConnectionState.Disconnected) {
            return;
        }

        this.savedOptions.set(hubConfiguration.hubName, hubConfiguration);
        let url = `${environment.apiUrl}/${hubConfiguration.hubName}`;

        if (hubConfiguration.queryParams) {
            const params = new URLSearchParams(hubConfiguration.queryParams).toString();
            url += `?${params}`;
        }
        const builder = new SignalR.HubConnectionBuilder();
        if (hubConfiguration.requireAuthentication) {
            builder.withUrl(url, { accessTokenFactory: () => this.tokenservice.getAccessToken() });
        } else {
            builder.withUrl(url);
        }
        const connection = builder.withAutomaticReconnect([500, 1000, 2000, 5000, 10000]).build();
        this.hubConnection.set(hubConfiguration.hubName, connection);

        if (!this.connectionStates.has(hubConfiguration.hubName)) {
            this.connectionStates.set(hubConfiguration.hubName, new BehaviorSubject<boolean>(false));
        }

        this.hubConnectionEvent(hubConfiguration.hubName);
        connection.start().then(() => {
            console.log("Connection Established");
            this.connectionStates.get(hubConfiguration.hubName)!.next(true);
        }).catch((err) => {
            console.log("Connection Failed " + err);
            const isUnAuthorized = err?.statusCode === 401 || err?.message?.includes('401') || err?.message?.toLowerCase().includes('unauthorized');
            if (isUnAuthorized && hubConfiguration.requireAuthentication) {
                console.log("Token expired during SignalR connection. Attempting refresh...");
                this.ids.refreshToken().subscribe({
                    next: (res: RefreshTokenResponse) => {
                        this.tokenservice.setAccessToken(res.response.engineIgnition);
                        this.tokenservice.setEngineValidationToken(res.response.engineValidation);
                        this.startConnection(this.savedOptions.get(hubConfiguration.hubName)!);
                    }, error: (err) => {
                        console.log(err);
                        this.tokenservice.clear();
                    }
                });
            } else {
                this.retryConnection(hubConfiguration.hubName);
            }
        });
    }

    hubConnectionEvent(hubName: string) {
        const connection = this.hubConnection.get(hubName)!;
        const state = this.connectionStates.get(hubName)!;
        connection.onreconnecting(() => {
            state.next(false);
            console.log(`${hubName} Reconnecting.`)
        });
        connection.onreconnected(() => {
            state.next(true);
            console.log(`${hubName} Reconnected.`);
        });
        connection.onclose(() => {
            state.next(false);
            console.log(`${hubName} Connection Closed`);
        })
    }

    private retryConnection(hubName: string) {
        setTimeout(() => {
            console.log(`${hubName}Retrying connection`);
            const options = this.savedOptions.get(hubName);
            if (options) {
                this.startConnection(options)
            }
        }, 5000);
    }

    subscribeHub<T>(hubName: string, eventName: string): Subject<T> {
        let hubEvents = this.eventMap.get(hubName);
        if (!hubEvents) {
            hubEvents = new Map<string, Subject<any>>();
            this.eventMap.set(hubName, hubEvents);
        }
        if (!hubEvents.has(eventName)) {
            const subject = new Subject<T>();
            hubEvents.set(eventName, subject);
            const connection = this.hubConnection.get(hubName)!;
            connection.on(eventName, (data: T) => { subject.next(data); });
        }
        return hubEvents.get(eventName)! as Subject<T>;
    }

    unsubscribeHub(hubName: string, eventName: string) {
        const hubEvents = this.eventMap.get(hubName);
        if (!hubEvents) {
            return;
        }
        const connection = this.hubConnection.get(hubName)!;
        connection.off(eventName);
        hubEvents.delete(eventName);
    }
}