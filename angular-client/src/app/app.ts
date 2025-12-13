import { Component, signal } from '@angular/core';
import { ImageGenerator } from './components/image-generator/image-generator';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ImageGenerator],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('angular-client');
}
