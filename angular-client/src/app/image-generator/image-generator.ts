import { Component } from '@angular/core';
import { ImageRequest } from '../models/image-request';
import { ImageGeneratorService } from '../services/image-generator-service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-image-generator',
  standalone: true,
  imports: [FormsModule,CommonModule],
  templateUrl: './image-generator.html',
  styleUrl: './image-generator.scss',
})
export class ImageGenerator {

  public generationRequest: ImageRequest = {
    prompt: '',
    quantity: 1, // Default a 1 immagine
    addExtraEffect: false 
  };
  
  public message: string = '';
  public isLoading: boolean = false;

  constructor(private imageGeneratorService: ImageGeneratorService) { }

  submitRequest() {
    this.isLoading = true;
    this.message = 'Invio richiesta...';

    // 1. Validazione per sicurezza
    if (!this.generationRequest.prompt) {
      this.message = 'Inserisci un prompt valido!';
      this.isLoading = false;
      return;
    }

    // 2. Chiamata al servizio
    this.imageGeneratorService.requestGeneration(this.generationRequest)
      .subscribe({
        next: (response) => {
          // La risposta arriva subito, perché la API ha messo in coda il task
          this.message = response.message + ' ' + response.details;
          this.isLoading = false;
          // Pulisci il prompt se vuoi
          this.generationRequest.prompt = ''; 
        },
        error: (err) => {
          this.message = 'ERRORE BACKEND: Impossibile contattare la API o RabbitMQ.';
          console.error(err);
          this.isLoading = false;
        }
      });
  }
}
