import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-share',
	imports: [],
	templateUrl: './share-svg.svg'
})
export class ShareSvg {
	@Input() width: string = '15px';
	@Input() height: string = '15px';
}