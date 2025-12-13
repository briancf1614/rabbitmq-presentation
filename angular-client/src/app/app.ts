import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ImageGenerator } from './image-generator/image-generator';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ImageGenerator],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('angular-client');
}
