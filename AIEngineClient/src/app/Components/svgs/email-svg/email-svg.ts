import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-svg-email',
  standalone: true,
  templateUrl: './email-svg.svg'
})
export class EmailSvg {
  @Input() width = '20px';
  @Input() height = '20px';
}
