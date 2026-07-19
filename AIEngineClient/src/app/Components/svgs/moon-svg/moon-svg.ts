import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-moon',
	imports: [],
	templateUrl: './moon-svg.svg'
})
export class MoonSvg {
	@Input() width: string = '16px';
	@Input() height: string = '16px';
}