export interface ImageRequest {
  prompt: string;
  quantity: number;
  addExtraEffect: boolean;
}

export interface ApiResponse {
  message: string;
  details: string;
  originalRequest: ImageRequest;
}