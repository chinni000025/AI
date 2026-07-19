import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-test-connection',
	imports: [],
	templateUrl: './test-connection-svg.svg'
})
export class TestConnectionSvg {
	@Input() width: string = '18px';
	@Input() height: string = '18px';
}