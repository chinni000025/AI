import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-info',
	imports: [],
	templateUrl: './info-svg.svg'
})
export class InfoSvg {
	@Input() height = '18px';
	@Input() width = '18px';
}
