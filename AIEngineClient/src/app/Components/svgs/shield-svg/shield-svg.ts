import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-shield',
	imports: [],
	templateUrl: './shield-svg.svg'
})
export class ShieldSvg {
	@Input() width = '20px';
	@Input() height = '20px';
}