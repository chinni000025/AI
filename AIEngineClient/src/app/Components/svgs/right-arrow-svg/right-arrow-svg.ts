import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-right-arrow',
	imports: [],
	templateUrl: './right-arrow-svg.svg'
})
export class RightArrowSvg {
	@Input() width: string = '18px';
	@Input() height: string = '18px';
}