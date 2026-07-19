import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-password-icon',
	standalone: true,
	templateUrl: './password-icon-svg.svg'
})
export class PasswordIconSvg {
	@Input() width = '20px';
	@Input() height = '20px';
}