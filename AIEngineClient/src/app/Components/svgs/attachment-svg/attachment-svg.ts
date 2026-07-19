import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-attachment',
	imports: [],
	templateUrl: './attachment-svg.svg'
})
export class AttachmentSvg {
	@Input() height = '22px';
	@Input() width = '22px';
}