export interface ProjectKnowledgeFile {
  id: string;
  fileName: string;
  fileSize: number;
  formattedSize: string;
  extension: string;
  uploadedAt: string;
}

export interface AIProject {
  id: string;
  name: string;
  description: string;
  icon: string;
  color: string;
  customInstructions: string;
  defaultModel: string;
  knowledgeFiles: ProjectKnowledgeFile[];
  createdAt: string;
  updatedAt: string;
  conversationCount: number;
}

export interface ProjectTemplate {
  name: string;
  icon: string;
  color: string;
  description: string;
  customInstructions: string;
}
