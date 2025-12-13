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

  // 1. STATO INIZIALE
  this.isLoading.set(true);
  this.generatedImageUrl.set(null); 
  this.responseMessage.set('Invio messaggio a RabbitMQ...'); // Feedback immediato

  const requestData: ImageRequest = {
    prompt: this.promptText(),
    quantity: 1,
    addExtraEffect: this.addExtraEffect()
  };

  // 2. CHIAMATA "FIRE & FORGET"
  this.imageService.requestGeneration(requestData).subscribe({
    next: (res) => {
      // 3. QUI È LA PROVA DI RABBITMQ!
      // Siamo arrivati qui in pochi millisecondi. L'API ci ha risposto SUBITO.
      // Se fosse sincrono, saremmo ancora bloccati ad aspettare.
      
      const apiRequestId = res.requestId; 
      
      // Diciamo all'utente che l'ordine è in coda
      this.responseMessage.set(`✅ Ordine preso in carico (Ticket: ${apiRequestId}). Il Worker ci sta lavorando...`);

      // Ora il frontend "dimentica" e controlla con calma ogni tanto
      this.waitForImage(apiRequestId);
    },
    error: (err) => {
      this.hasError.set(true);
      this.responseMessage.set('❌ Errore: RabbitMQ non raggiungibile.');
      this.isLoading.set(false);
    }
  });
}

  waitForImage(id: string) {
  const url = `${this.imageService.getImageBaseUrl()}/${id}_0.jpg`;
  
  let tentativi = 0; // Contatore
  const maxTentativi = 30; // 30 tentativi * 2 sec = 60 secondi massimo

  const interval = setInterval(() => {
    tentativi++; // Aumentiamo il contatore

    fetch(url, { method: 'HEAD' })
      .then(res => {
        if (res.ok) {
          // CASO 1: TROVATA!
          clearInterval(interval); // Stop
          this.generatedImageUrl.set(url); // Mostra foto
          this.isLoading.set(false); // Sblocca tasti
          this.responseMessage.set('Fatto! Ecco la tua immagine.');
        }
      })
      .catch(() => { /* Errore di rete, ignoriamo e riproviamo */ });

    // CASO 2: TROPPO TEMPO (TIMEOUT)
    if (tentativi >= maxTentativi) {
       clearInterval(interval); // Stop forzato
       this.isLoading.set(false);
       this.hasError.set(true);
       this.responseMessage.set('Timeout: Il worker ci sta mettendo troppo (o si è bloccato).');
    }

  }, 2000); // Ogni 2 secondi
}
}
