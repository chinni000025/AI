import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ThemeService } from '../../services/theme.service';
import { Router } from '@angular/router';
import { TokenService } from '../../services/token-service';
import { LoaderService } from '../../services/loader-service';
import { IdentityService } from '../../services/identity-service';
import { finalize } from 'rxjs';
import { SnackbarService } from '../../services/snackbar-service';
import { EngineRoutes } from '../../services/engine-route-constants';
import { BrandLogoSvg } from "../svgs/brand-logo-svg/brand-logo-svg";
import { UserSvg } from "../svgs/user-svg/user-svg";
import { EmailSvg } from "../svgs/email-svg/email-svg";
import { PasswordIconSvg } from "../svgs/password-icon-svg/password-icon-svg";
import { ConfirmPasswordIconSvg } from "../svgs/confirm-password-icon-svg/confirm-password-icon-svg";
import { ShowPasswordEyeSvg } from "../svgs/show-password-eye-svg/show-password-eye-svg";
import { HidePasswordEyeSvg } from "../svgs/hide-password-eye-svg/hide-password-eye-svg";
import { SunSvg } from "../svgs/sun-svg/sun-svg";
import { MoonSvg } from "../svgs/moon-svg/moon-svg";
import { RightArrowSvg } from "../svgs/right-arrow-svg/right-arrow-svg";
import { GoogleSvg } from "../svgs/google-svg/google-svg";
import { MicrosoftSvg } from "../svgs/microsoft-svg/microsoft-svg";
import { passwordValidator } from '../shared/password-validator';

@Component({
    selector: 'app-identity',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        BrandLogoSvg,
        UserSvg,
        EmailSvg,
        PasswordIconSvg,
        ConfirmPasswordIconSvg,
        ShowPasswordEyeSvg,
        HidePasswordEyeSvg,
        SunSvg,
        MoonSvg,
        RightArrowSvg,
        GoogleSvg,
        MicrosoftSvg,
        FormsModule
    ],
    templateUrl: './identity.html',
    styleUrl: './identity.css',
})
export class Identity {
    loginForm: FormGroup;
    signUpForm: FormGroup;
    activeTab = 'login';

    constructor(
        private fb: FormBuilder,
        public themeService: ThemeService,
        private router: Router,
        private tokenService: TokenService,
        private loader: LoaderService,
        private ids: IdentityService,
        private snack: SnackbarService) {

        this.loginForm = this.fb.group({
            userName: ['', [Validators.required]],
            password: ['', [Validators.required]]
        });

        this.signUpForm = this.fb.group(
            {
                userName: ['', [Validators.required]],
                email: ['', [Validators.required, Validators.email]],
                password: ['', [Validators.required, passwordValidator()]],
                confirmPassword: ['', [Validators.required]]
            },
            {
                validators: this.passwordMatchValidator()
            }
        );
    }

    get loginControls() {
        return this.loginForm.controls;
    }

    get signUpControls() {
        return this.signUpForm.controls;
    }

    forgetIdentity() {
        this.router.navigate([EngineRoutes.ForgetIdentity]);
    }

    submitLoginForm(): void {
        this.loginForm.markAllAsTouched();
        if (this.loginForm.invalid) {
            return;
        }

        const payload = {
            UserName: this.loginForm.value.userName,
            Password: this.loginForm.value.password,
            SessionId: this.tokenService.ensureSessionId()
        }

        this.loader.show("Logging in to the AI Engine");
        this.ids.login(payload)
            .pipe(finalize(() => { this.loader.hide() }))
            .subscribe(
                {
                    next: (res: any) => {
                        this.snack.showSuccessMessage("Welcome to the Engine", 5000);
                        this.tokenService.setAccessToken(res.response.engineIgnition);
                        this.tokenService.setEngineValidationToken(res.response.engineValidation);
                        this.router.navigate([EngineRoutes.PromptSpace], { replaceUrl: true });
                    },
                    error: (err) => {
                        this.snack.showErrorMessage(err.error);
                    }
                }
            );
    }

    submitSignUpForm(): void {
        this.signUpForm.markAllAsTouched();

        if (this.signUpForm.invalid) {
            return;
        }
        const payload = {
            UserName: this.signUpForm.value.userName,
            Email: this.signUpForm.value.email,
            Password: this.signUpForm.value.password,
            ConfirmPassword: this.signUpForm.value.confirmPassword
        }

        this.loader.show("Registering to the AI Engine");
        this.ids.register(payload)
            .pipe(finalize(() => { this.loader.hide() }))
            .subscribe(
                {
                    next: (res) => {
                        this.signUpForm.reset();
                        this.snack.showSuccessMessage("Registration successful. Please log in to access the AI Engine.", 5000);
                        this.router.navigate(['']);
                        this.activeTab = 'login';
                    },
                    error: (err) => {
                        this.snack.showErrorMessage(err.error);
                    }
                }
            );
    }

    private passwordMatchValidator(): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            const password = control.get('password')?.value;
            const confirmPassword = control.get('confirmPassword')?.value;

            if (!password || !confirmPassword) {
                return null;
            }
            return password === confirmPassword ? null : { passwordMismatch: true };
        };
    }
}
