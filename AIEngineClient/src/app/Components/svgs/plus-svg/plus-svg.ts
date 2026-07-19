import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-plus',
	imports: [],
	templateUrl: './plus-svg.svg'
})
export class PlusSvg {
	@Input() height = '18px';
	@Input() width = '18px';
}
