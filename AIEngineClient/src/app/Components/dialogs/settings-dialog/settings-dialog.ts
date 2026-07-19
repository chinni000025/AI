import { Component, EventEmitter, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SettingsSvg } from '../../svgs/settings-svg/settings-svg';
import { GoogleDriveSvg } from '../../svgs/google-drive-svg/google-drive-svg';
import { DropdownSvg } from '../../svgs/dropdown-svg/dropdown-svg';
import { GoogleConnectionConstans } from '../../../services/engine-route-constants';
import { TokenService } from '../../../services/token-service';
import { SettingsService } from '../../../settings-service';
import { SnackbarService } from '../../../services/snackbar-service';
import { provideCloudinaryLoader } from '@angular/common';

@Component({
    selector: 'app-settings-dialog',
    imports: [FormsModule, SettingsSvg, GoogleDriveSvg, DropdownSvg],
    templateUrl: './settings-dialog.html',
    styleUrl: './settings-dialog.css'
})
export class SettingsDialog {

    @Output() closed = new EventEmitter<void>();

    /** Active settings tab */
    activeTab: 'connections' | 'engine' | 'security' | 'smtp' | 'ingestion' | 'logs' = 'connections';

    /** Connections state */
    expandedConnection: string | null = null;

    selectedAuthMethod: { [key: string]: string } = {
        'google-drive': 'oauth'
    };

    googleClientId = '';
    googleClientSecret = ''; // only for storing.
    /** SMTP settings */
    smtpHost = '';
    smtpPort = '587';
    smtpUsername = '';
    smtpPassword = '';
    smtpUseSsl: boolean = true;

    readonly connections = [
        {
            id: 'google-drive',
            name: 'Google Drive',
            description: 'Connect your Google Drive to access files, documents, and collaborate seamlessly.',
            status: 'disconnected' as 'connected' | 'disconnected',
            authMethods: [
                { id: 'oauth', label: 'OAuth 2.0', icon: 'shield', description: 'Sign in with Google for delegated user access.' }
            ]
        }
    ];

    /** Engine settings – Regional */
    selectedRegion = 'us-east-1';
    selectedLanguage = 'en-US';
    selectedTimezone = 'UTC';
    selectedTimeFormat: '12h' | '24h' = '12h';

    /** Engine settings – Security & Lifecycle */
    tokenExpiryTime = '60';
    linkShareExpiration = '7';
    recycleBinRetention = '30';

    readonly regions = [
        { value: 'us-east-1', label: 'US East (N. Virginia)' },
        { value: 'us-west-2', label: 'US West (Oregon)' },
        { value: 'eu-west-1', label: 'EU West (Ireland)' },
        { value: 'eu-central-1', label: 'EU Central (Frankfurt)' },
        { value: 'ap-south-1', label: 'Asia Pacific (Mumbai)' },
        { value: 'ap-southeast-1', label: 'Asia Pacific (Singapore)' },
        { value: 'ap-northeast-1', label: 'Asia Pacific (Tokyo)' }
    ];

    readonly languages = [
        { value: 'en-US', label: 'English (US)' },
        { value: 'en-GB', label: 'English (UK)' },
        { value: 'es-ES', label: 'Spanish (Spain)' },
        { value: 'fr-FR', label: 'French (France)' },
        { value: 'de-DE', label: 'German (Germany)' },
        { value: 'ja-JP', label: 'Japanese (Japan)' },
        { value: 'zh-CN', label: 'Chinese (Simplified)' },
        { value: 'hi-IN', label: 'Hindi (India)' }
    ];

    readonly timezones = [
        { value: 'UTC', label: 'UTC (Coordinated Universal Time)' },
        { value: 'America/New_York', label: 'Eastern Time (ET)' },
        { value: 'America/Chicago', label: 'Central Time (CT)' },
        { value: 'America/Denver', label: 'Mountain Time (MT)' },
        { value: 'America/Los_Angeles', label: 'Pacific Time (PT)' },
        { value: 'Europe/London', label: 'Greenwich Mean Time (GMT)' },
        { value: 'Europe/Berlin', label: 'Central European Time (CET)' },
        { value: 'Asia/Kolkata', label: 'India Standard Time (IST)' },
        { value: 'Asia/Tokyo', label: 'Japan Standard Time (JST)' }
    ];

    readonly tokenExpiryOptions = [
        { value: '15', label: '15 minutes' },
        { value: '30', label: '30 minutes' },
        { value: '60', label: '1 hour' },
        { value: '120', label: '2 hours' },
        { value: '480', label: '8 hours' },
        { value: '1440', label: '24 hours' },
        { value: '10080', label: '7 days' }
    ];

    readonly linkExpirationOptions = [
        { value: '1', label: '1 day' },
        { value: '3', label: '3 days' },
        { value: '7', label: '7 days' },
        { value: '14', label: '14 days' },
        { value: '30', label: '30 days' },
        { value: '90', label: '90 days' },
        { value: '0', label: 'Never' }
    ];

    readonly recycleBinOptions = [
        { value: '1', label: '1 day' },
        { value: '7', label: '7 days' },
        { value: '14', label: '14 days' },
        { value: '24', label: '24 hours' },
        { value: '30', label: '30 days' },
        { value: '60', label: '60 days' },
        { value: '90', label: '90 days' }
    ];
    /** Ingestion settings */
    ingestionUrls: { url: string; depth: string; status: 'idle' | 'ingesting' | 'completed' | 'failed' }[] = [];
    newIngestionUrl = '';
    newIngestionDepth = '2';

    readonly crawlDepthOptions = [
        { value: '1', label: '1 level (page only)' },
        { value: '2', label: '2 levels' },
        { value: '3', label: '3 levels' },
        { value: '5', label: '5 levels (deep)' },
        { value: '0', label: 'Unlimited' }
    ];

    /** Custom dropdown state */
    activeDropdown: string | null = null;

    /** Log settings */
    selectedLogLevel = 'info';

    readonly logLevels = [
        { value: 'trace', label: 'Trace', color: '#94a3b8', description: 'Most detailed diagnostic info, including all events.' },
        { value: 'debug', label: 'Debug', color: '#60a5fa', description: 'Detailed information for diagnosing problems.' },
        { value: 'info', label: 'Info', color: '#22c55e', description: 'General operational events and confirmations.' },
        { value: 'warn', label: 'Warning', color: '#f59e0b', description: 'Potential issues that may need attention.' },
        { value: 'error', label: 'Error', color: '#ef4444', description: 'Failures that need immediate investigation.' },
        { value: 'fatal', label: 'Fatal', color: '#dc2626', description: 'Critical failures causing system shutdown.' }
    ];

    readonly logPriorityOrder = ['trace', 'debug', 'info', 'warn', 'error', 'fatal'];

    constructor(private tokenService: TokenService, private settingService: SettingsService, private snack: SnackbarService) { }

    close(): void {
        this.closed.emit();
    }

    setTab(tab: 'connections' | 'engine' | 'security' | 'smtp' | 'ingestion' | 'logs'): void {
        this.activeTab = tab;
    }

    toggleConnection(connectionId: string): void {
        this.expandedConnection = this.expandedConnection === connectionId ? null : connectionId;
    }

    selectAuthMethod(connectionId: string, methodId: string): void {
        this.selectedAuthMethod[connectionId] = methodId;
    }

    getSelectedLogLevelIndex(): number {
        return this.logPriorityOrder.indexOf(this.selectedLogLevel);
    }

    isLogLevelActive(level: string): boolean {
        const selectedIndex = this.logPriorityOrder.indexOf(this.selectedLogLevel);
        const levelIndex = this.logPriorityOrder.indexOf(level);
        return levelIndex >= selectedIndex;
    }

    selectLogLevel(level: string): void {
        this.selectedLogLevel = level;
    }

    /** Ingestion methods */
    addIngestionUrl(): void {
        const url = this.newIngestionUrl.trim();
        if (url) {
            this.ingestionUrls = [...this.ingestionUrls, { url, depth: this.newIngestionDepth, status: 'idle' }];
            this.newIngestionUrl = '';
        }
    }

    removeIngestionUrl(index: number): void {
        this.ingestionUrls = this.ingestionUrls.filter((_, i) => i !== index);
    }

    /** Custom dropdown methods */
    toggleDropdown(id: string, event: MouseEvent): void {
        event.stopPropagation();
        this.activeDropdown = this.activeDropdown === id ? null : id;
    }

    selectDropdownOption(field: string, value: string): void {
        (this as any)[field] = value;
        this.activeDropdown = null;
    }

    closeDropdowns(): void {
        this.activeDropdown = null;
    }

    getOptionLabel(options: { value: string; label: string }[], value: string): string {
        return options.find(o => o.value === value)?.label || value;
    }

    get isValid(): boolean {
        return !!(this.googleClientId && this.googleClientSecret);
    }

    saveAndConnect() {
        this.settingService.saveGoogleConnection(this.googleClientId, this.googleClientSecret).subscribe({
            next: () => {
                this.snack.showSuccessMessage("Connecction saved Successfully");
            },
            error: (err: any) => {
                this.snack.showErrorMessage(err.error);
            }
        });
    }

    openConsentScreen() {
        const oauthUrl =
            'https://accounts.google.com/o/oauth2/v2/auth'
            + '?client_id=' + encodeURIComponent(this.googleClientId)
            + '&redirect_uri=' + encodeURIComponent(GoogleConnectionConstans.redirectUri)
            + '&response_type=code'
            + '&access_type=offline'
            + '&prompt=consent'
            + '&state=' + encodeURIComponent(this.getUserId())
            + '&scope=' + encodeURIComponent(GoogleConnectionConstans.scopes.join(' '));
        const width = 500;
        const height = 650;
        const left = window.screenX + (window.outerWidth - width) / 2;
        const top = window.screenY + (window.outerHeight - height) / 2;

        window.open(
            oauthUrl,
            'GoogleConsent',
            `width=${width},height=${height},left=${left},top=${top},popup=yes`
        );
    }

    getUserId(): any {
        const token = this.tokenService.getAccessToken();

        if (!token) {
            return null;
        }
        const payload = JSON.parse(atob(token.split('.')[1]));
        return payload.sub;
    }

    sendTestEmail() {
        const payload = {
            Host: this.smtpHost,
            User: this.smtpUsername,
            Password: this.smtpPassword,
            Port: this.smtpPort,
            EnableSSL: this.smtpUseSsl
        };
        this.settingService.sendTestEmail(payload).subscribe({
            next: () => {
                this.snack.showSuccessMessage("Test Mail sent successfully");
            },
            error: (err: any) => {
                this.snack.showErrorMessage(err.error);
            }
        });
    }

    saveSmtpSettings() {
        const payload = {
            Host: this.smtpHost,
            User: this.smtpUsername,
            Password: this.smtpPassword,
            Port: this.smtpPort,
            EnableSSL: this.smtpUseSsl
        };
        this.settingService.saveSmtpSettings(payload).subscribe({
            next: () => {
                this.snack.showSuccessMessage("saved smtp settings");
            },
            error: (err: any) => {
                this.snack.showErrorMessage(err.error);
            }
        });
    }
}
