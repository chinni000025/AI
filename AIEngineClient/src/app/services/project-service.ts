import { inject, Injectable } from '@angular/core';
import { EngineCore } from './engine-core';
import { EngineControllers } from './engine-route-constants';
import { AIProject } from '../models/project';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  private http = inject(EngineCore);

  constructor() { }

  createProject(aiProject: AIProject) {
    return this.http.post(`${EngineControllers.ConversationController}/createProject`, aiProject);
  }

  getProjects() {
    return this.http.get(`${EngineControllers.ConversationController}/getUserProjects`);
  }

  getProjectConversation(projectId: string) {
    return this.http.get(`${EngineControllers.ConversationController}/getConversationProject/${projectId}`,);
  }
}
