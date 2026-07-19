import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-svg-trash',
  imports: [],
  templateUrl: './trash-svg.svg'
})
export class TrashSvg {
  @Input() width: string = '15px';
  @Input() height: string = '15px';
}
