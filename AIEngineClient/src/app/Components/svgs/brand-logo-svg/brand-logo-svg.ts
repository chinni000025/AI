import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-brand-logo',
	standalone: true,
	templateUrl: './brand-logo-svg.svg'
})
export class BrandLogoSvg {
	@Input() width: string = '100%';
	@Input() height: string = '100%';
}