import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-settings',
	imports: [],
	templateUrl: './settings-svg.svg'
})
export class SettingsSvg {
	@Input() width = '16px';
	@Input() height = '16px';
}