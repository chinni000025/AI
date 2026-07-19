import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-port',
	imports: [],
	templateUrl: './port-svg.svg'
})
export class PortSvg {
	@Input() width = '20px';
	@Input() height = '20px';
}