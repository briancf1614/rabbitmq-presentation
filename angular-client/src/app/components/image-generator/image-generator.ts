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
  quantity = signal<number>(1);         
  addExtraEffect = signal<boolean>(false); 

  // --- STATE SIGNALS ---
  // CORREZIONE: Aggiunto generatedImageUrl
  generatedImageUrl = signal<string | null>(null);
  isLoading = signal(false);
  responseMessage = signal<string | null>(null);
  hasError = signal(false);

  generateImage() {
  this.isLoading.set(true);
  this.generatedImageUrl.set(null); // Pulisce vecchia immagine

  // 1. Inviamo richiesta
  const requestData: ImageRequest = {
    // requestId: NON lo mettiamo, perché ora è opzionale (?)
    prompt: this.promptText(),
    quantity: this.quantity(),
    addExtraEffect: this.addExtraEffect() // <--- AGGIUNGI QUESTO (anche se è false)
  };
  
  this.imageService.requestGeneration(requestData).subscribe({
    next: (res) => {
      // 2. L'API ci dice: "Ok, il tuo ID è XYZ"
      this.responseMessage.set('In lavorazione...');
      
      // 3. Iniziamo ad aspettare l'immagine XYZ
      this.waitForImage(res.requestId); 
    },
    error: () => { /* gestisci errore */ }
  });
}

  waitForImage(id: string) {
  // L'URL sarà questo
  const url = `${this.imageService.getImageBaseUrl()}/${id}_0.jpg`;
  
  // Polling semplice
  const interval = setInterval(() => {
    fetch(url, { method: 'HEAD' }) // Chiede solo "esisti?"
      .then(res => {
        if (res.ok) {
          clearInterval(interval); // Ferma il timer
          this.generatedImageUrl.set(url); // Mostra l'immagine
          this.isLoading.set(false);
          this.responseMessage.set('Fatto!');
        }
      })
      .catch(() => { /* Ancora nulla, aspettiamo il prossimo giro */ });
  }, 2000);
}
}
