import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-speaker',
	imports: [],
	templateUrl: './speaker-svg.svg'
})
export class SpeakerSvg {
	@Input() width: string = '18px';
	@Input() height: string = '18px';
}
