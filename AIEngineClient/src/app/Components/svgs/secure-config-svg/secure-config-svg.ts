import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-secure-config',
	imports: [],
	templateUrl: './secure-config-svg.svg'
})
export class SecureConfigSvg {
	@Input() width = '18px';
	@Input() height = '18px';
}