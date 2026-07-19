import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-error',
	imports: [],
	templateUrl: './error-svg.svg'
})
export class ErrorSvg {
	@Input() height = '18px';
	@Input() width = '18px';
}
