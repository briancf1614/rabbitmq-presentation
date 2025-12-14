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
    // 1. IMPRIME ESTO EN LA CONSOLA (F12) PARA VERIFICAR
    console.log('Respuesta de la API:', res); 

    // 2. Extraemos el ID de la respuesta
    // Si en la consola ves 'requestId', úsalo aquí. 
    // Si ves 'RequestId' (mayúscula), cámbialo aquí.
    const apiRequestId = res.requestId; 

    if (!apiRequestId) {
      console.error('¡La API no devolvió un ID!');
      this.responseMessage.set('Error: ID no recibido.');
      return;
    }

    this.responseMessage.set(`Procesando ID: ${apiRequestId}...`);

    // 3. Pasamos ese ID a la función de espera
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
