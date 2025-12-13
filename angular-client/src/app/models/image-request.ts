export interface ImageRequest {
  requestId?: string;
  prompt: string;
  quantity: number;
  addExtraEffect: boolean;
}

export interface ApiResponse {
  message: string;
  requestId: string;
  details?: string;
}
export interface CardData {
  id: number;
  title: string;
  description: string;
  status: 'active' | 'inactive' | 'pending';
  imageUrl?: string; // opzionale
}