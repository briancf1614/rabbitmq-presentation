export interface ImageRequest {
  requestId: string | undefined; // <--- NUOVO
  prompt: string;
  quantity: number;
  addExtraEffect: boolean;
}

export interface ApiResponse {
  requestId: string; // <--- MODIFICATO
  message: string;
  details: string;
  originalRequest: ImageRequest;
}
export interface CardData {
  id: number;
  title: string;
  description: string;
  status: 'active' | 'inactive' | 'pending';
  imageUrl?: string; // opzionale
}