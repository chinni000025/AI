import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-warning',
	imports: [],
	templateUrl: './warning-svg.svg'
})
export class WarningSvg {
	@Input() height = '18px';
	@Input() width = '18px';
}
