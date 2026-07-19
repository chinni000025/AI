import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-verified',
	imports: [],
	templateUrl: './verified-svg.svg'
})
export class VerifiedSvg {
	@Input() height = '20px';
	@Input() width = '20px';
}
