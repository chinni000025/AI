import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-google-drive',
	imports: [],
	templateUrl: './google-drive-svg.svg'
})
export class GoogleDriveSvg {
	@Input() width = '16px';
	@Input() height = '16px';
}
