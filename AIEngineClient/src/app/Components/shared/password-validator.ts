import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function passwordValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        const password = control.value as string;

        if (!password) {
            return null;
        }

        const errors: ValidationErrors = {};

        if (password.length < 8 || password.length > 64) {
            errors['passwordLength'] = true;
        }

        if (!/[A-Z]/.test(password)) {
            errors['missingUppercase'] = true;
        }

        if (!/[a-z]/.test(password)) {
            errors['missingLowercase'] = true;
        }

        if (!/\d/.test(password)) {
            errors['missingNumber'] = true;
        }

        if (!/[^A-Za-z0-9\s]/.test(password)) {
            errors['missingSpecialCharacter'] = true;
        }

        if (/^[^A-Za-z0-9]/.test(password)) {
            errors['startsWithSpecialCharacter'] = true;
        }

        if (/\s/.test(password)) {
            errors['containsSpace'] = true;
        }

        return Object.keys(errors).length ? errors : null;
    };
}
