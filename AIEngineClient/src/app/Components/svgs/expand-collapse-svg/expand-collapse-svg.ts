import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-expand-collapse',
	imports: [],
	templateUrl: './expand-collapse-svg.svg'
})
export class ExpandCollapseSvg {
	@Input() transform: string = 'rotate(0deg)';
	@Input() width: string = '14px';
	@Input() height: string = '14px';
}
