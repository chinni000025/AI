import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-close',
	imports: [],
	templateUrl: './close-svg.svg'
})
export class CloseSvg {
	@Input() height = '18px';
	@Input() width = '18px';
}