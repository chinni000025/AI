import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-rename',
	imports: [],
	templateUrl: './rename-svg.svg'
})
export class RenameSvg {
	@Input() height = '15px';
	@Input() width = '15px';
}
