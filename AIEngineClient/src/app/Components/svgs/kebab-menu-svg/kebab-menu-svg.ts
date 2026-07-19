import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-kebab-menu',
	imports: [],
	templateUrl: './kebab-menu-svg.svg'
})
export class KebabMenuSvg {
	@Input() width: string = '14px';
	@Input() height: string = '14px';
}
