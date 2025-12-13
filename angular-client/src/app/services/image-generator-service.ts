import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse, ImageRequest } from '../models/image-request';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.prod';

@Injectable({
  providedIn: 'root',
})
export class ImageGeneratorService {
  private http = inject(HttpClient);
  private endpoint = `${environment.apiUrl}/api/ImageRequest/generate`;

  requestGeneration(request: ImageRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(this.endpoint, request);
  }
  
  // Helper per ottenere l'URL base per le immagini (utile per il componente)
  getImageBaseUrl(): string {
    return `${environment.apiUrl}/images`;
  }
}