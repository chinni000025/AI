import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-chats',
	imports: [],
	templateUrl: './chats-svg.svg'
})
export class ChatsSvg {
	@Input() width: string = '15px';
	@Input() height: string = '15px';
}