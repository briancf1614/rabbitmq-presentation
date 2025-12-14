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
  generatedImages = signal<string[]>([]);
  isLoading = signal(false);
  responseMessage = signal<string | null>(null);
  hasError = signal(false);

  generateImage() {
  if (!this.promptText() || this.isLoading()) return;

  // 1. STATO INIZIALE
  this.isLoading.set(true); 
  this.responseMessage.set('Invio messaggio a RabbitMQ...'); // Feedback immediato
  this.hasError.set(false);
  
  const requestData: ImageRequest = {
    prompt: this.promptText(),
    quantity: this.quantity(),
    addExtraEffect: this.addExtraEffect()
  };

  // 2. CHIAMATA "FIRE & FORGET"
  this.imageService.requestGeneration(requestData).subscribe({
  next: (res) => {
    // 1. IMPRIME ESTO EN LA CONSOLA (F12) PARA VERIFICAR
    console.log('Respuesta de la API:', res); 

    // 2. Extraemos el ID de la respuesta
    // Si en la consola ves 'requestId', úsalo aquí. 
    // Si ves 'RequestId' (mayúscula), cámbialo aquí.
    const apiRequestId = res.requestId; 

    if (!apiRequestId) {
      this.hasError.set(true);
      this.responseMessage.set('Error: ID non ricevuto.');
      this.isLoading.set(false);
      return;
    }

    this.responseMessage.set(`Procesando ID: ${apiRequestId}...`);

    // 3. Pasamos ese ID a la función de espera
    this.waitForImages(apiRequestId); 
  },
    error: (err) => {
      this.hasError.set(true);
      this.responseMessage.set('❌ Errore: RabbitMQ non raggiungibile.');
      this.isLoading.set(false);
    }
  });
}

  waitForImages(id: string) {
  const base = this.imageService.getImageBaseUrl();
  const qty = this.quantity();

  // urls esperadas: /images/{id}_0.jpg, /images/{id}_1.jpg ...
  const urls = Array.from({ length: qty }, (_, i) => `${base}/${id}_${i}.jpg`);

  let attempts = 0;
  const maxAttempts = 30;

  const checkImage = (url: string) =>
    new Promise<boolean>((resolve) => {
      const img = new Image();

      img.onload = () => resolve(true);
      img.onerror = () => resolve(false);

      // anti-cache para que no te devuelva una versión vieja / 404 cacheada
      img.src = `${url}?t=${Date.now()}`;
    });

  const interval = setInterval(async () => {
    attempts++;

    // comprobamos todas las imágenes en paralelo
    const results = await Promise.all(urls.map(checkImage));
    const allReady = results.every(Boolean);

    if (allReady) {
      clearInterval(interval);

      const ts = Date.now();
      const finalUrls = urls.map(u => `${u}?t=${ts}`);

      // agrega todas al inicio (la más reciente arriba)
      this.generatedImages.update(list => [...finalUrls.reverse(), ...list]);

      this.isLoading.set(false);
      this.hasError.set(false);
      this.responseMessage.set('✨ Immagini pronte e aggiunte alla galleria!');
      return;
    }

    if (attempts >= maxAttempts) {
      clearInterval(interval);
      this.isLoading.set(false);
      this.hasError.set(true);
      this.responseMessage.set('Timeout: tarda troppo.');
    }
  }, 2000);
}
}
