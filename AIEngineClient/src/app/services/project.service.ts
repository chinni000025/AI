import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { AIProject, ProjectKnowledgeFile, ProjectTemplate } from '../models/project';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private readonly STORAGE_KEY = 'ai_engine_projects';
  private readonly ACTIVE_PROJECT_KEY = 'ai_engine_active_project_id';

  private projectsSubject = new BehaviorSubject<AIProject[]>([]);
  public projects$: Observable<AIProject[]> = this.projectsSubject.asObservable();

  private activeProjectSubject = new BehaviorSubject<AIProject | null>(null);
  public activeProject$: Observable<AIProject | null> = this.activeProjectSubject.asObservable();

  // Curated templates like modern AI websites (Claude / ChatGPT)
  readonly projectTemplates: ProjectTemplate[] = [
    {
      name: 'Full-Stack Software Architect',
      icon: '🚀',
      color: '#00f0ff',
      description: 'System design, clean architecture, Angular & .NET code generation and security reviews.',
      customInstructions: 'You are an elite Principal Software Architect. Always provide clean, production-ready code with strong typing, explicit error handling, and scalable design patterns. Adhere to modular Clean Architecture principles.'
    },
    {
      name: 'Data & Market Research',
      icon: '📊',
      color: '#8b5cf6',
      description: 'Deep quantitative analysis, trend synthesis, financial metrics and executive summaries.',
      customInstructions: 'You are a Senior Strategic Research Analyst. Synthesize information with high precision, highlight critical risk factors, structure findings with clear tables/bullet points, and avoid unverified speculation.'
    },
    {
      name: 'Technical Documentation & Docs',
      icon: '📚',
      color: '#10b981',
      description: 'API reference manuals, architecture decision records (ADRs), and user guides.',
      customInstructions: 'You are a Lead Technical Writer. Write comprehensive, well-structured documentation in Markdown. Include diagrams using Mermaid syntax, clear prerequisites, and reproducible examples.'
    },
    {
      name: 'Cybersecurity & Code Audit',
      icon: '🛡️',
      color: '#f43f5e',
      description: 'Vulnerability assessments, OWASP compliance, threat modeling and hardening checklists.',
      customInstructions: 'You are a Principal Security Auditor. Analyze code and architecture for security vulnerabilities (injection, auth bypass, data leaks, privilege escalation). Provide concrete remediation diffs and CVSS score estimates.'
    }
  ];

  constructor() {
    this.initializeProjects();
  }

  private initializeProjects(): void {
    const saved = localStorage.getItem(this.STORAGE_KEY);
    if (saved) {
      try {
        const parsed: AIProject[] = JSON.parse(saved);
        this.projectsSubject.next(parsed);
      } catch {
        this.seedDefaultProjects();
      }
    } else {
      this.seedDefaultProjects();
    }

    const activeId = localStorage.getItem(this.ACTIVE_PROJECT_KEY);
    if (activeId) {
      const match = this.projectsSubject.value.find(p => p.id === activeId);
      if (match) {
        this.activeProjectSubject.next(match);
      }
    }
  }

  private seedDefaultProjects(): void {
    const defaults: AIProject[] = [
      {
        id: 'proj-architect-01',
        name: 'Full-Stack System Architect',
        icon: '🚀',
        color: '#00f0ff',
        description: 'Enterprise AI & Distributed Systems engineering, Clean Architecture, and high-performance APIs.',
        customInstructions: 'You are an elite Principal Architect. Adhere to Clean Architecture, SOLID principles, and high-performance reactive paradigms.',
        defaultModel: 'gemini-3.5-flash',
        knowledgeFiles: [
          {
            id: 'file-spec-1',
            fileName: 'ai-engine-architecture-spec.md',
            fileSize: 48 * 1024,
            formattedSize: '48 KB',
            extension: 'md',
            uploadedAt: new Date(Date.now() - 86400000 * 2).toISOString()
          }
        ],
        createdAt: new Date(Date.now() - 86400000 * 5).toISOString(),
        updatedAt: new Date(Date.now() - 86400000).toISOString(),
        conversationCount: 4
      },
      {
        id: 'proj-research-02',
        name: 'Enterprise Market Intelligence',
        icon: '📊',
        color: '#8b5cf6',
        description: 'Competitive benchmarking, AI vector embedding performance benchmarks and operational metrics.',
        customInstructions: 'Act as a Senior Research Analyst. Deliver concise executive briefs with high data density and structured markdown summaries.',
        defaultModel: 'gemini-3.5-flash',
        knowledgeFiles: [],
        createdAt: new Date(Date.now() - 86400000 * 3).toISOString(),
        updatedAt: new Date(Date.now() - 86400000 * 2).toISOString(),
        conversationCount: 1
      }
    ];

    this.saveProjects(defaults);
  }

  get projects(): AIProject[] {
    return this.projectsSubject.value;
  }

  get activeProject(): AIProject | null {
    return this.activeProjectSubject.value;
  }

  setActiveProject(project: AIProject | null): void {
    this.activeProjectSubject.next(project);
    if (project) {
      localStorage.setItem(this.ACTIVE_PROJECT_KEY, project.id);
    } else {
      localStorage.removeItem(this.ACTIVE_PROJECT_KEY);
    }
  }

  createProject(params: {
    name: string;
    description: string;
    icon: string;
    color: string;
    customInstructions: string;
    defaultModel: string;
    files?: File[];
  }): AIProject {
    const now = new Date().toISOString();
    const knowledgeFiles: ProjectKnowledgeFile[] = (params.files || []).map(file => ({
      id: 'file-' + Math.random().toString(36).substring(2, 9),
      fileName: file.name,
      fileSize: file.size,
      formattedSize: this.formatBytes(file.size),
      extension: this.extractExtension(file.name),
      uploadedAt: now
    }));

    const newProject: AIProject = {
      id: 'proj-' + Math.random().toString(36).substring(2, 10),
      name: params.name.trim(),
      description: params.description.trim(),
      icon: params.icon || '🚀',
      color: params.color || '#00f0ff',
      customInstructions: params.customInstructions.trim(),
      defaultModel: params.defaultModel || 'gemini-3.5-flash',
      knowledgeFiles,
      createdAt: now,
      updatedAt: now,
      conversationCount: 0
    };

    const updated = [newProject, ...this.projects];
    this.saveProjects(updated);
    this.setActiveProject(newProject);
    return newProject;
  }

  updateProject(id: string, updates: Partial<AIProject>): AIProject | null {
    const list = [...this.projects];
    const index = list.findIndex(p => p.id === id);
    if (index === -1) return null;

    list[index] = {
      ...list[index],
      ...updates,
      updatedAt: new Date().toISOString()
    };

    this.saveProjects(list);
    if (this.activeProject?.id === id) {
      this.activeProjectSubject.next(list[index]);
    }
    return list[index];
  }

  deleteProject(id: string): void {
    const filtered = this.projects.filter(p => p.id !== id);
    this.saveProjects(filtered);
    if (this.activeProject?.id === id) {
      this.setActiveProject(null);
    }
  }

  attachFilesToProject(projectId: string, files: File[]): void {
    const project = this.projects.find(p => p.id === projectId);
    if (!project) return;

    const now = new Date().toISOString();
    const newFiles: ProjectKnowledgeFile[] = files.map(f => ({
      id: 'file-' + Math.random().toString(36).substring(2, 9),
      fileName: f.name,
      fileSize: f.size,
      formattedSize: this.formatBytes(f.size),
      extension: this.extractExtension(f.name),
      uploadedAt: now
    }));

    this.updateProject(projectId, {
      knowledgeFiles: [...project.knowledgeFiles, ...newFiles]
    });
  }

  removeKnowledgeFile(projectId: string, fileId: string): void {
    const project = this.projects.find(p => p.id === projectId);
    if (!project) return;

    this.updateProject(projectId, {
      knowledgeFiles: project.knowledgeFiles.filter(f => f.id !== fileId)
    });
  }

  incrementConversationCount(projectId: string): void {
    const project = this.projects.find(p => p.id === projectId);
    if (!project) return;
    this.updateProject(projectId, {
      conversationCount: (project.conversationCount || 0) + 1
    });
  }

  private saveProjects(projects: AIProject[]): void {
    this.projectsSubject.next(projects);
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(projects));
  }

  private formatBytes(bytes: number, decimals = 1): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }

  private extractExtension(fileName: string): string {
    const parts = fileName.split('.');
    return parts.length > 1 ? parts.pop()!.toLowerCase() : 'file';
  }
}
