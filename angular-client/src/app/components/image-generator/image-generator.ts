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

  promptText = signal('');
  quantity = signal<number>(1);
  useRabbit = signal<boolean>(true);

  generatedImages = signal<string[]>([]);
  isSubmitting = signal(false);        // 👈 nuovo: blocca solo l’invio
  responseMessage = signal<string | null>(null);
  hasError = signal(false);

  generateImage() {
    if (!this.promptText() || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.hasError.set(false);

    // ❌ NON pulire la gallery, sennò ogni richiesta cancella le precedenti
    // this.generatedImages.set([]);

    const requestData: ImageRequest = {
      prompt: this.promptText(),
      quantity: this.quantity(),
      useRabbit: this.useRabbit(),
    };

    const start = performance.now();
    this.responseMessage.set(this.useRabbit()
      ? 'Invio job a RabbitMQ... (puoi inviare altre richieste)'
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
          this.isSubmitting.set(false);
          return;
        }

        // ✅ QUI: sblocca subito la UI se Rabbit è ON
        // (anche in direct puoi sbloccare, ma lì di solito vuoi evitare spam)
        this.isSubmitting.set(false);

        this.responseMessage.set(`RequestId: ${apiRequestId} — sto scaricando le immagini...`);
        this.waitForImages(apiRequestId, start);
      },
      error: () => {
        this.hasError.set(true);
        this.responseMessage.set('❌ Errore: backend non raggiungibile.');
        this.isSubmitting.set(false);
      }
    });
  }

  waitForImages(id: string, startTime: number) {
    const base = this.imageService.getImageBaseUrl();
    const qty = this.quantity();
    const urls = Array.from({ length: qty }, (_, i) => `${base}/${id}_${i}.jpg`);
    const seen = new Set<number>();

    let attempts = 0;
    const maxAttempts = 80;

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
          this.responseMessage.set(`✨ Pronta: ${seen.size}/${qty} (RequestId: ${id})`);
        }
      });

      if (seen.size === qty) {
        clearInterval(interval);
        this.hasError.set(false);
        const seconds = ((performance.now() - startTime) / 1000).toFixed(1);
        this.responseMessage.set(`✅ Completato in ${seconds}s (RabbitMQ: ${this.useRabbit() ? 'ON' : 'OFF'})`);
        return;
      }

      if (attempts >= maxAttempts) {
        clearInterval(interval);
        this.hasError.set(true);
        this.responseMessage.set(`Timeout: pronte ${seen.size}/${qty}. (RequestId: ${id})`);
      }
    }, 1200);
  }
}