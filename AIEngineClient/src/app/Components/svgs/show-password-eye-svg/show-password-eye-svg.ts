import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-show-password-eye',
	standalone: true,
	templateUrl: './show-password-eye-svg.svg'
})
export class ShowPasswordEyeSvg {
	@Input() width = '18px';
	@Input() height = '18px';
}