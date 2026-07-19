import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ThemeService } from '../../services/theme.service';
import { BrandLogoSvg } from '../svgs/brand-logo-svg/brand-logo-svg';
import { EmailSvg } from '../svgs/email-svg/email-svg';
import { MoonSvg } from '../svgs/moon-svg/moon-svg';
import { RightArrowSvg } from '../svgs/right-arrow-svg/right-arrow-svg';
import { SunSvg } from '../svgs/sun-svg/sun-svg';
import { LeftArrowSvg } from "../svgs/left-arrow-svg/left-arrow-svg";
import { IdentityService } from '../../services/identity-service';
import { LoaderService } from '../../services/loader-service';
import { SnackbarService } from '../../services/snackbar-service';
import { finalize } from 'rxjs';
import { Router } from '@angular/router';

@Component({
    selector: 'app-forgot-identity',
    imports: [CommonModule, ReactiveFormsModule, BrandLogoSvg, EmailSvg, SunSvg, MoonSvg, RightArrowSvg, LeftArrowSvg],
    templateUrl: './forgot-identity.html',
    styleUrl: './forgot-identity.css',
})
export class ForgotIdentity {
    forgotForm: FormGroup;

    constructor(private fb: FormBuilder, public themeService: ThemeService, private router: Router,
        private ids: IdentityService, private loader: LoaderService, private snack: SnackbarService) {
        this.forgotForm = this.fb.group({
            email: ['', [Validators.required, Validators.email]]
        });
    }

    get forgotControls() {
        return this.forgotForm.controls;
    }

    backToLogIn() {
        this.router.navigate(['']);
    }

    submitForgotForm(): void {
        this.forgotForm.markAllAsTouched();

        if (this.forgotForm.invalid) {
            return;
        }
        const payload = {
            Email: this.forgotForm.value.email
        };
        this.loader.show("Sending Reset Link to the Registered Mail")
        this.ids.forgetIdentity(payload)
            .pipe(finalize(() => { this.loader.hide() })).subscribe({
                next: () => {
                    this.snack.showSuccessMessage("Reset Link Sent Successfully");
                },
                error: (err) => {
                    this.snack.showErrorMessage(err);
                }
            });

    }
}