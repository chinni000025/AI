import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-clock',
	imports: [],
	templateUrl: './clock-svg.svg'
})
export class ClockSvg {
	@Input() width: string = '24px';
	@Input() height: string = '24px';
}