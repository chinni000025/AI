export interface SnackbarConfig {
  message: string;
  type: any;
  duration?: number;
  position?: any;
}

export interface ChatMessage {
  id: string;
  role: string;
  content: any;
  timestamp: string;
}

export interface Conversation {
  conversationId: string;
  conversationTitle: string;
  isPinned: boolean;
  isFavorite: boolean,
  lastMessageAt: boolean,
  createdAt: string;
  modelUsed?: string;
}

export interface RefreshTokenResponse {
  response: {
    engineIgnition: string;
    engineValidation: string;
  };
}

export interface UploadFileTask {
  id: string;
  fileName: string;
  fileSize: number;
  formattedSize: string;
  uploadedBytes: number;
  progress: number;
  speed: string;
  status: 'uploading' | 'completed' | 'failed' | 'paused';
  extension: string;
}