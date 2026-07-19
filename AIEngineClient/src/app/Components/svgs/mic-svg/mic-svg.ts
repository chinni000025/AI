import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-mic',
	imports: [],
	templateUrl: './mic-svg.svg'
})
export class MicSvg {
	@Input() height = '22px';
	@Input() width = '22px';
}