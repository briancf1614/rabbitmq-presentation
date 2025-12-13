import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiResponse, ImageRequest } from '../models/image-request';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ImageGeneratorService {
    private apiUrl = 'http://localhost:8080/api/ImageRequest/generate';

  constructor(private http: HttpClient) { }

  /**
   * Invia la richiesta di generazione immagine alla API .NET
   * @param request L'oggetto che contiene il prompt e le opzioni.
   * @returns Un Observable con la risposta della API (che è subito un "OK").
   */
  requestGeneration(request: ImageRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(this.apiUrl, request);
  }
  
}
