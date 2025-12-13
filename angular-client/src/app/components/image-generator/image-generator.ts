import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiResponse, ImageRequest } from '../../models/image-request';
import { ImageGeneratorService } from '../../services/image-generator-service';

@Component({
  selector: 'app-image-generator',
  imports: [FormsModule,CommonModule],
  templateUrl: './image-generator.html',
  styleUrl: './image-generator.scss',
})
export class ImageGenerator {
private imageService = inject(ImageGeneratorService);

  // --- INPUT SIGNALS ---
  promptText = signal(''); 
  quantity = signal<number>(1);         // Default 1 immagine
  addExtraEffect = signal<boolean>(false); // Default false

  // --- STATE SIGNALS ---
  isLoading = signal(false);
  responseMessage = signal<string | null>(null);
  hasError = signal(false);

  generateImage() {
    // Validazione base
    if (!this.promptText() || this.isLoading()) return;

    this.isLoading.set(true);
    this.responseMessage.set(null);
    this.hasError.set(false);

    // Costruiamo l'oggetto esattamente come lo vuole l'interfaccia
    const requestData: ImageRequest = {
      prompt: this.promptText(),
      quantity: this.quantity(),
      addExtraEffect: this.addExtraEffect()
    };

    this.imageService.requestGeneration(requestData).subscribe({
      next: (res: ApiResponse) => {
        // Usa la risposta tipizzata
        this.responseMessage.set(res.message || 'Successo!'); 
        this.isLoading.set(false);
        // Qui potresti anche resettare il form se vuoi
      },
      error: (err) => {
        console.error(err);
        this.hasError.set(true);
        this.responseMessage.set('Errore durante la generazione.');
        this.isLoading.set(false);
      }
    });
  }
}
