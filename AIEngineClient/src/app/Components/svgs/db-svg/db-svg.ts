import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-db',
	imports: [],
	templateUrl: './db-svg.svg',
})
export class DbSvg {
	@Input() height = '20px';
	@Input() width = '20px';
}
