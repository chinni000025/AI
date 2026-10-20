export interface AIProject {
  name: string;
  description: string;
}

export interface AIProjectInfo {
  id: string;
  name: string;
  createAt: Date;
  description: string;
  isArchieved: boolean;
  updatedAt: Date;
}