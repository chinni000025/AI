import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-send',
	imports: [],
	templateUrl: './send-svg.svg'
})
export class SendSvg {
	@Input() width: string = '22px';
	@Input() height: string = '22px';
}