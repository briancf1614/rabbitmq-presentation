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
  useRabbit = signal<boolean>(true);


  // --- STATE SIGNALS ---
  // CORREZIONE: Aggiunto generatedImageUrl
  generatedImages = signal<string[]>([]);
  isLoading = signal(false);
  responseMessage = signal<string | null>(null);
  hasError = signal(false);

  generateImage() {
    if (!this.promptText() || this.isLoading()) return;

    this.isLoading.set(true);
    this.hasError.set(false);
    this.generatedImages.set([]); // opzionale: pulisci gallery a ogni run

    const requestData: ImageRequest = {
      prompt: this.promptText(),
      quantity: this.quantity(),
      useRabbit: this.useRabbit(),
    };

    const start = performance.now();
    this.responseMessage.set(this.useRabbit()
      ? 'Invio job a RabbitMQ...'
      : 'Generazione diretta (senza RabbitMQ)...');

    const call$ = this.useRabbit()
      ? this.imageService.requestGenerationRabbit(requestData)
      : this.imageService.requestGenerationDirect(requestData);

    call$.subscribe({
      next: (res: ApiResponse) => {
        const apiRequestId = res.requestId;

        if (!apiRequestId) {
          this.hasError.set(true);
          this.responseMessage.set('Errore: ID non ricevuto.');
          this.isLoading.set(false);
          return;
        }

        this.responseMessage.set(`RequestId: ${apiRequestId} — attendo immagini...`);
        this.waitForImages(apiRequestId, start);
      },
      error: () => {
        this.hasError.set(true);
        this.responseMessage.set('❌ Errore: backend non raggiungibile.');
        this.isLoading.set(false);
      }
    });
  }

  waitForImages(id: string, startTime: number) {
    const base = this.imageService.getImageBaseUrl();
    const qty = this.quantity();

    const urls = Array.from({ length: qty }, (_, i) => `${base}/${id}_${i}.jpg`);
    const seen = new Set<number>();

    let attempts = 0;
    const maxAttempts = 80; // per demo con 10 immagini meglio più alto

    const checkImage = (url: string) =>
      new Promise<boolean>((resolve) => {
        const img = new Image();
        img.onload = () => resolve(true);
        img.onerror = () => resolve(false);
        img.src = `${url}?t=${Date.now()}`;
      });

    const interval = setInterval(async () => {
      attempts++;

      const results = await Promise.all(urls.map(checkImage));

      results.forEach((ok, i) => {
        if (ok && !seen.has(i)) {
          seen.add(i);
          const finalUrl = `${urls[i]}?t=${Date.now()}`;
          this.generatedImages.update(list => [finalUrl, ...list]);
          this.responseMessage.set(`✨ Pronta: ${seen.size}/${qty}`);
        }
      });

      if (seen.size === qty) {
        clearInterval(interval);
        this.isLoading.set(false);
        this.hasError.set(false);

        const seconds = ((performance.now() - startTime) / 1000).toFixed(1);
        this.responseMessage.set(`✅ Completato in ${seconds}s (RabbitMQ: ${this.useRabbit() ? 'ON' : 'OFF'})`);
        return;
      }

      if (attempts >= maxAttempts) {
        clearInterval(interval);
        this.isLoading.set(false);
        this.hasError.set(true);
        this.responseMessage.set(`Timeout: pronte ${seen.size}/${qty}.`);
      }
    }, 1200);
  }
}