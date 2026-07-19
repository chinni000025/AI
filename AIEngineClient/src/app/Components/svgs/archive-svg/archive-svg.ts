import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-archive',
	imports: [],
	templateUrl: './archive-svg.svg'
})
export class ArchiveSvg {
	@Input() height = '15px';
	@Input() width = '15px';
}
