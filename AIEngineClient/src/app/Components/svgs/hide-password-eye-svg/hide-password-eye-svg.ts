import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-hide-password-eye',
	standalone: true,
	templateUrl: './hide-password-eye-svg.svg'
})
export class HidePasswordEyeSvg {
	@Input() width: string = '18px';
	@Input() height: string = '18px';
}