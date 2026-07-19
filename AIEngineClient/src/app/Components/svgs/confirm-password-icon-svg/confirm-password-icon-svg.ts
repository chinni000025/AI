import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-confirm-password-icon',
	standalone: true,
	templateUrl: './confirm-password-icon-svg.svg'
})
export class ConfirmPasswordIconSvg {
	@Input() width = '20px';
	@Input() height = '20px';
}