import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-svg-engine-drive',
  standalone: true,
  imports: [],
  templateUrl: './engine-drive-svg.svg'
})
export class EngineDriveSvg {
  @Input() width = '16px';
  @Input() height = '16px';
}
