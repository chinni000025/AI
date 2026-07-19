import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-logout',
	imports: [],
	templateUrl: './logout-svg.svg'
})
export class LogoutSvg {
	@Input() width = '16px';
	@Input() height = '16px';
}