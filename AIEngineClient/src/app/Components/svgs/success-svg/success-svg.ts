import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-success',
	imports: [],
	templateUrl: './success-svg.svg'
})
export class SuccessSvg {
	@Input() height = '18px';
	@Input() width = '18px';
}