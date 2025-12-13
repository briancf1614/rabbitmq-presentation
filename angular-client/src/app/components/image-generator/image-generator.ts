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
    if (!this.promptText() || this.isLoading()) return;

    this.isLoading.set(true);
    this.responseMessage.set(null);
    this.generatedImageUrl.set(null);
    this.hasError.set(false);

    // Prepariamo l'oggetto come da tuo modello
    const requestData: ImageRequest = {
      // Nota: RequestId lo mettiamo null o opzionale, lo genera l'API (come deciso prima)
      requestId: undefined, 
      prompt: this.promptText(),
      quantity: this.quantity(),
      addExtraEffect: this.addExtraEffect()
    };

    // USIAMO IL TUO SERVICE
    this.imageService.requestGeneration(requestData).subscribe({
      next: (res) => {
        // L'API risponde con { message: "...", requestId: "GUID" }
        // Se il tuo modello ApiResponse ha 'requestId', usalo qui:
        const apiRequestId = res.requestId; 
        
        this.responseMessage.set(`Inviato! ID: ${apiRequestId}. Attendo worker...`);

        // Avviamo il polling
        this.waitForImage(apiRequestId);
      },
      error: (err) => {
        console.error(err);
        this.hasError.set(true);
        this.responseMessage.set('Errore: Impossibile contattare il server.');
        this.isLoading.set(false);
      }
    });
  }

  waitForImage(id: string | undefined) {
    if (!id) return;

    // Recuperiamo l'URL base dal service (es. https://api.briancico.com/images)
    const baseUrl = this.imageService.getImageBaseUrl();
    const expectedUrl = `${baseUrl}/${id}_0.jpg`;

    let attempts = 0;
    const maxAttempts = 30; 

    const interval = setInterval(() => {
      attempts++;
      
      // HEAD request per vedere se il file esiste
      fetch(expectedUrl, { method: 'HEAD' })
        .then(res => {
          if (res.ok) {
            clearInterval(interval);
            this.generatedImageUrl.set(expectedUrl);
            this.isLoading.set(false);
            this.responseMessage.set('Immagine pronta!');
          }
        })
        .catch(() => {});

      if (attempts >= maxAttempts) {
        clearInterval(interval);
        this.isLoading.set(false);
        this.hasError.set(true);
        this.responseMessage.set('Timeout: Il worker è troppo lento o offline.');
      }
    }, 2000); 
  }
}
