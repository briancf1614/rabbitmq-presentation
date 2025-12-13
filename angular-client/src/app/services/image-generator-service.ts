import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiResponse, ImageRequest } from '../models/image-request';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ImageGeneratorService {
  private apiUrl = '/api/ImageRequest/generate';
  private http = inject(HttpClient);

  requestGeneration(request: ImageRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(this.apiUrl, request);
  }
  
}
