import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-configuration-profile',
	imports: [],
	templateUrl: './configuration-profile-svg.svg'
})
export class ConfigurationProfileSvg {
	@Input() width = '20px';
	@Input() height = '20px';
}