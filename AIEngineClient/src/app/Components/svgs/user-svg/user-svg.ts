import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-user',
	standalone: true,
	templateUrl: './user-svg.svg'
})
export class UserSvg {
	@Input() width = '20px';
	@Input() height = '20px';
}