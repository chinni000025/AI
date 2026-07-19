import { Component, Input, input } from '@angular/core';

@Component({
	selector: 'app-svg-credentials',
	imports: [],
	templateUrl: './credentials-svg.svg',
})
export class CredentialsSvg {
	@Input() width = '20px';
	@Input() height = '20px';
}