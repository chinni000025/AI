import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-dropdown',
	imports: [],
	templateUrl: './dropdown-svg.svg'
})
export class DropdownSvg {
	@Input() width: string = '16px';
	@Input() height: string = '16px';
}