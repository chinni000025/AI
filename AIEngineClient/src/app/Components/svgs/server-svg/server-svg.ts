import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-server',
	imports: [],
	templateUrl: './server-svg.svg',
})
export class ServerSvg {
	@Input() width = '20px';
	@Input() height = '20px';
}