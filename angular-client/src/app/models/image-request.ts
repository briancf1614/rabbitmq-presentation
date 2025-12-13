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
export interface CardData {
  id: number;
  title: string;
  description: string;
  status: 'active' | 'inactive' | 'pending';
  imageUrl?: string; // opzionale
}