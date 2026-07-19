import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { SnackbarService } from '../../services/snackbar-service';
import { EngineService } from '../../services/engine-service';
import { LoaderService } from '../../services/loader-service';
import { ChangeDetectorRef } from '@angular/core';
import { ThemeService } from '../../services/theme.service';
import { BrandLogoSvg } from "../svgs/brand-logo-svg/brand-logo-svg";
import { ConfigurationProfileSvg } from '../svgs/configuration-profile-svg/configuration-profile-svg';
import { CredentialsSvg } from "../svgs/credentials-svg/credentials-svg";
import { PasswordIconSvg } from "../svgs/password-icon-svg/password-icon-svg";
import { ShieldSvg } from '../svgs/shield-svg/shield-svg';
import { SecureConfigSvg } from "../svgs/secure-config-svg/secure-config-svg";
import { ServerSvg } from "../svgs/server-svg/server-svg";
import { PortSvg } from "../svgs/port-svg/port-svg";
import { DbSvg } from "../svgs/db-svg/db-svg";
import { UserSvg } from "../svgs/user-svg/user-svg";
import { VerifiedSvg } from "../svgs/verified-svg/verified-svg";
import { ClockSvg } from "../svgs/clock-svg/clock-svg";
import { ShowPasswordEyeSvg } from "../svgs/show-password-eye-svg/show-password-eye-svg";
import { HidePasswordEyeSvg } from "../svgs/hide-password-eye-svg/hide-password-eye-svg";
import { RightArrowSvg } from "../svgs/right-arrow-svg/right-arrow-svg";
import { TestConnectionSvg } from "../svgs/test-connection-svg/test-connection-svg";
import { SunSvg } from "../svgs/sun-svg/sun-svg";
import { MoonSvg } from "../svgs/moon-svg/moon-svg";
import { Route, Router } from '@angular/router';
import { DataBaseProvider, EngineConstants, EngineRoutes } from '../../services/engine-route-constants';

@Component({
    selector: 'app-engine-setup',
    imports: [CommonModule, ReactiveFormsModule, BrandLogoSvg, ConfigurationProfileSvg, CredentialsSvg, ShieldSvg,
        PasswordIconSvg, SecureConfigSvg, ServerSvg, PortSvg, DbSvg, UserSvg, VerifiedSvg,
        ClockSvg, ShowPasswordEyeSvg, HidePasswordEyeSvg, RightArrowSvg, TestConnectionSvg, SunSvg, MoonSvg],
    templateUrl: './engine-setup.html',
    styleUrl: './engine-setup.css',
})
export class EngineSetup {
    engineSetupForm: FormGroup;
    connectionStatus: 'idle' | 'success' | 'error' = 'idle';
    selectedProvider: 'mssql' | 'postgres' = 'mssql';

    constructor(
        private fb: FormBuilder,
        private snackbar: SnackbarService,
        private engineService: EngineService,
        private loader: LoaderService,
        private cdr: ChangeDetectorRef,
        public themeService: ThemeService,
        private router: Router
    ) {
        this.engineSetupForm = this.fb.group({
            Server: ['', [Validators.required]],
            Port: [1433, [Validators.required, Validators.min(1000)]],
            DataBaseName: ['', [Validators.required]],
            UserName: ['', [Validators.required]],
            Password: ['', [Validators.required]]
        });
    }

    get engineSetupControls() {
        return this.engineSetupForm.controls;
    }

    selectProvider(provider: 'mssql' | 'postgres') {
        if (this.selectedProvider == provider) {
            return;
        }
        this.selectedProvider = provider;
        const currentPortNumber = this.engineSetupForm.get("Port")?.value;
        if (!currentPortNumber || currentPortNumber == 1433 || currentPortNumber == 5432) {
            this.engineSetupForm.patchValue({ Port: provider == 'mssql' ? 1433 : 5432 });
        }
    }

    configureEngineDatabase() {
        this.engineSetupForm.markAllAsTouched();

        if (this.engineSetupForm.invalid) {
            return;
        }

        const payload = this.getEngineSetupPayload();
        this.loader.show('Configuring Engine Database');

        this.engineService.configuredEngineDataBase(payload)
            .pipe(finalize(() => this.loader.hide()))
            .subscribe({
                next: () => {
                    this.snackbar.showSuccessMessage('Engine Configured Successfully');
                    this.router.navigate([EngineRoutes.Processing], { replaceUrl: true });
                },
                error: (err) => {
                    this.snackbar.showErrorMessage(err.error);
                }
            });
    }

    testConnection() {
        this.engineSetupForm.markAllAsTouched();

        if (this.engineSetupForm.invalid) {
            this.connectionStatus = 'error';
            this.cdr.detectChanges();
            return;
        }

        const payload = this.getEngineSetupPayload();
        this.loader.show('Testing Engine Connection');

        this.engineService.testConnection(payload)
            .pipe(
                finalize(() => this.loader.hide())
            ).subscribe({
                next: () => {
                    this.connectionStatus = 'success';
                    this.snackbar.showSuccessMessage('Test Connection Successfully');
                    this.cdr.detectChanges();
                },
                error: (err) => {
                    this.snackbar.showErrorMessage(err.error);
                    this.connectionStatus = 'error';
                    this.cdr.detectChanges();
                }
            });
    }

    private getEngineSetupPayload() {
        const formValue = this.engineSetupForm.getRawValue();
        return {
            ...formValue,
            DataBaseType: this.selectedProvider == "mssql" ? DataBaseProvider.SqlServer
                : DataBaseProvider.PostgreSql
        };
    }
}
