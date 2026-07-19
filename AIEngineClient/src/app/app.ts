import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Snackbar } from './Components/snackbar/snackbar';
import { Loader } from './Components/loader/loader';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Snackbar, Loader],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('AIEngineClient');

  constructor() { }
  ngOnInit() { }
}
