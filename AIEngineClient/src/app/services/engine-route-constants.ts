import { Injectable } from '@angular/core';
import { DialogButton } from '../Components/dialogs/confirmation-dialog/confirmation-dialog';
import { StreamInvocationMessage } from '@microsoft/signalr';

@Injectable({
    providedIn: 'root',
})
export class EngineControllers {
    public static readonly IdentitiyController = "Identity";
    public static readonly EngineStatusController = "Engine";
    public static readonly ConversationController = "Conversation";
    public static readonly EngineStateController = "EngineState";
    public static readonly DashboardController = "Dashboard";
    public static readonly ConnectionController = "Connection";
    public static readonly EncryptionController = "Encryption";
}

export class EngineRoutes {
    public static readonly PromptSpace = "prompt-space";
    public static readonly EngineSetup = "engine-setup";
    public static readonly ForgetIdentity = "forgot-identity";
    public static readonly ResetIdentity = "reset-identity";
    public static readonly Processing = "processing";
}

export class EngineConstants {
    public static AccessToken = "EngineIgnition";
    public static readonly EngineValidationToken = "EngineValidationToken";
    public static readonly EngineVerification = "EngineVerification";
    public static readonly EngineKeyToken = "Engine-Key-Token";
    public static SessionId = "sessionId";
    public static ForceLogout = "ForceLogout";
    public static EngineResponse = "EngineResponse";
    public static EngineStateChanged = "EngineStateChange";

    //For Snackbar
    public static Success = "Success";
    public static Error = "Error";
    public static Info = "Info";
    public static Warning = "Warning";

    public static Top = "Top";
    public static Bottom = "Bottom";

    public static User = "User";
    public static Assistant = "Assistant";
    public static ConversationId = "id";
}

export enum DataBaseProvider {
    SqlServer = 1,
    PostgreSql = 2
}

export class Buttons {
    public static readonly Ok: DialogButton = {
        text: 'Ok',
        value: true,
        variant: 'info'
    };

    public static readonly Cancel: DialogButton = {
        text: 'Cancel',
        value: false,
        variant: 'secondary'
    };

    public static readonly Confirm: DialogButton = {
        text: 'Confirm',
        value: true,
        variant: 'primary'
    };

    public static readonly Delete: DialogButton = {
        text: 'Delete',
        value: true,
        variant: 'danger'
    };
}

export interface ModelProvider {
    name: string;
    models: string[];
}

export class VoiceStatusLabel {
    public static readonly Listening = 'listening';
    public static readonly Processing = 'processing';
    public static readonly Done = 'done';
}

export interface CatalogModel {
    value: string;
    displayName: string;
}

export interface CatalogProvider {
    name: string;
    models: CatalogModel[];
}

export interface PaginationResponse<T> {
    Item: T[],
    TotalCount: number,
    Page: number,
    PageSize: number,
}

export interface ArchiveChat {
    conversationId: string,
    title: string,
    preview: string,
    messageCount: number,
    archivedAt: Date
}

//Google Integeration Scopes.

export class GoogleConnectionConstans {
    //'https://www.googleapis.com/auth/spreadsheets' --> for sheets.
    public static readonly scopes = [
        'https://www.googleapis.com/auth/drive'
    ];
    public static readonly redirectUri = `${window.location.origin}/api/Connection/oauth/google/callback`;
}

export interface HubConfiguration {
    hubName: string;
    requireAuthentication?: boolean;
    queryParams?: Record<string, string>;
}

export class HubEndpoints {
    public static readonly NotificationHub = "notificationHub";
    public static readonly EngineStatusHub = "engineStatusHub";
}

