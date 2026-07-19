import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { BrandLogoSvg } from "../svgs/brand-logo-svg/brand-logo-svg";
import { ShowPasswordEyeSvg } from "../svgs/show-password-eye-svg/show-password-eye-svg";
import { HidePasswordEyeSvg } from "../svgs/hide-password-eye-svg/hide-password-eye-svg";
import { PasswordIconSvg } from "../svgs/password-icon-svg/password-icon-svg";
import { ConfirmPasswordIconSvg } from "../svgs/confirm-password-icon-svg/confirm-password-icon-svg";
import { LeftArrowSvg } from '../svgs/left-arrow-svg/left-arrow-svg';
import { RightArrowSvg } from '../svgs/right-arrow-svg/right-arrow-svg';
import { IdentityService } from '../../services/identity-service';
import { SnackbarService } from '../../services/snackbar-service';
import { LoaderService } from '../../services/loader-service';
import { finalize } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { ThemeService } from '../../services/theme.service';
import { SunSvg } from '../svgs/sun-svg/sun-svg';
import { MoonSvg } from '../svgs/moon-svg/moon-svg';

@Component({
    selector: 'app-reset-identity',
    imports: [
        CommonModule,
        ReactiveFormsModule,
        BrandLogoSvg,
        ShowPasswordEyeSvg,
        HidePasswordEyeSvg,
        PasswordIconSvg,
        ConfirmPasswordIconSvg,
        LeftArrowSvg,
        RightArrowSvg,
        SunSvg,
        MoonSvg
    ],
    templateUrl: './reset-identity.html',
    styleUrl: './reset-identity.css',
})
export class ResetIdentity {
    resetForm: FormGroup;
    email: any;
    token: any;

    constructor(private fb: FormBuilder, private ids: IdentityService,
        private snack: SnackbarService, private loader: LoaderService, private router: Router, private route: ActivatedRoute,
        public themeService: ThemeService) {
        this.resetForm = this.fb.group(
            {
                newPassword: ['', [Validators.required, Validators.minLength(6)]],
                confirmPassword: ['', [Validators.required]]
            },
            {
                validators: this.passwordMatchValidator()
            }
        );
    }

    ngOnInit() {
        this.route.queryParams.subscribe(params => {
            this.email = params['email'];
            this.token = params['token'];
        });
    }

    get resetControls() {
        return this.resetForm.controls;
    }

    submitResetForm(): void {
        this.resetForm.markAllAsTouched();

        if (this.resetForm.invalid) {
            return;
        }

        const payload = {
            Email: this.email,
            Token: this.token,
            NewPassword: this.resetForm.value.newPassword,
        };
        this.loader.show("Reseting Password");
        this.ids.resetIdentity(payload).pipe(finalize(() => {
            this.loader.hide();
        })).subscribe({
            next: () => {
                this.snack.showSuccessMessage("Password updated successfully");
                this.router.navigate(['']);
            }, error: (err) => {
                this.snack.showErrorMessage(err.error);
            }
        });

    }

    private passwordMatchValidator(): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            const password = control.get('newPassword')?.value;
            const confirmPassword = control.get('confirmPassword')?.value;

            if (!password || !confirmPassword) {
                return null;
            }

            return password === confirmPassword ? null : { passwordMismatch: true };
        };
    }
}
