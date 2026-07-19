import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-pin',
	imports: [],
	templateUrl: './pin-svg.svg'
})
export class PinSvg {
	@Input() height = '15px';
	@Input() width = '15px';
}
