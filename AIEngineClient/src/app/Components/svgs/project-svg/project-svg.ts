import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-project',
	imports: [],
	templateUrl: './project-svg.svg'
})
export class ProjectSvg {
	@Input() height = '15px';
	@Input() width = '15px';
}
