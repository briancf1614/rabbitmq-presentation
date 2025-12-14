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
  private rabbitEndpoint = `${environment.apiUrl}/api/ImageRequest/generate`;
  private directEndpoint = `${environment.apiUrl}/api/ImageRequest/generate-direct`;

  requestGenerationRabbit(request: ImageRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(this.rabbitEndpoint, request);
  }

  requestGenerationDirect(request: ImageRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(this.directEndpoint, request);
  }
  // Helper per ottenere l'URL base per le immagini (utile per il componente)
  getImageBaseUrl(): string {
    return `${environment.apiUrl}/images`;
  }
}