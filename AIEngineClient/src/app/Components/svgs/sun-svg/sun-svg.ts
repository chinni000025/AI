import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-sun',
	imports: [],
	templateUrl: './sun-svg.svg'
})
export class SunSvg {
	@Input() width: string = '16px';
	@Input() height: string = '16px';
}
