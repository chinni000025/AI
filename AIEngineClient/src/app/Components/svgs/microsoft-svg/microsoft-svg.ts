import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-microsoft',
	imports: [],
	templateUrl: './microsoft-svg.svg'
})
export class MicrosoftSvg {
	@Input() width: string = '20px';
	@Input() height: string = '20px';
}