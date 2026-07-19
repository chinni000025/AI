import { Component, Input } from '@angular/core';

@Component({
	selector: 'app-svg-favorite',
	imports: [],
	templateUrl: './favorite-svg.svg'
})
export class FavoriteSvg {
	@Input() fillColor: string = 'none';
	@Input() width: string = '15px';
	@Input() height: string = '15px';
}
