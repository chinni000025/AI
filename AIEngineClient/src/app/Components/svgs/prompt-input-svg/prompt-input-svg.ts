import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-prompt-input',
	imports: [],
	templateUrl: './prompt-input-svg.svg'
})
export class PromptInputSvg {
	@Input() height = '20px';
	@Input() width = '20px';
}
