import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-google',
	imports: [],
	templateUrl: './google-svg.svg'
})
export class GoogleSvg {
	@Input() width: string = '20px';
	@Input() height: string = '20px';
}