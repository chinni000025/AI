import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-thunder',
	imports: [],
	templateUrl: './thunder-svg.svg'
})
export class ThunderSvg {
	@Input() width = '16px';
	@Input() height = '16px';
}
