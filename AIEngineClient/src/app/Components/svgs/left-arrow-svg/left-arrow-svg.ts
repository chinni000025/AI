import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-left-arrow',
	imports: [],
	templateUrl: './left-arrow-svg.svg'
})
export class LeftArrowSvg {
	@Input() width: string = '16px';
	@Input() height: string = '16px';
}