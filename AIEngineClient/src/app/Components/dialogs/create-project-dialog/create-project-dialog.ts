import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '../../../services/project.service';
import { AIProject, ProjectTemplate } from '../../../models/project';

@Component({
  selector: 'app-create-project-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-project-dialog.html',
  styleUrl: './create-project-dialog.css'
})
export class CreateProjectDialog {
  @Output() closed = new EventEmitter<void>();
  @Output() created = new EventEmitter<AIProject>();

  projectName = '';
  projectDescription = '';
  selectedIcon = '📁';
  selectedColor = '#00f0ff';
  customInstructions = '';
  selectedModel = 'gemini-3.5-flash';
  nameError = '';

  selectedFiles: File[] = [];
  isDragOver = false;

  readonly availableIcons = ['📁', '🚀', '⚡', '🧠', '🛠️', '📊', '💡', '🔬', '🛡️', '🌐', '🤖', '📚'];

  readonly availableColors = [
    '#00f0ff', // Cyan
    '#8b5cf6', // Violet
    '#0072ff', // Blue
    '#10b981', // Emerald
    '#f59e0b', // Amber
    '#f43f5e', // Rose
    '#ec4899', // Pink
    '#14b8a6'  // Teal
  ];

  templates: ProjectTemplate[] = [];

  constructor(private projectService: ProjectService) {
    this.templates = this.projectService.projectTemplates;
  }

  applyTemplate(tmpl: ProjectTemplate): void {
    this.projectName = tmpl.name;
    this.selectedIcon = tmpl.icon;
    this.selectedColor = tmpl.color;
    this.projectDescription = tmpl.description;
    this.customInstructions = tmpl.customInstructions;
    this.nameError = '';
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.addFiles(Array.from(input.files));
      input.value = '';
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.addFiles(Array.from(event.dataTransfer.files));
    }
  }

  private addFiles(files: File[]): void {
    // Avoid duplicates by name
    const existingNames = new Set(this.selectedFiles.map(f => f.name));
    const unique = files.filter(f => !existingNames.has(f.name));
    this.selectedFiles = [...this.selectedFiles, ...unique];
  }

  removeSelectedFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  submitCreateProject(): void {
    if (!this.projectName.trim()) {
      this.nameError = 'Project name is required';
      return;
    }

    const newProject = this.projectService.createProject({
      name: this.projectName,
      description: this.projectDescription,
      icon: this.selectedIcon,
      color: this.selectedColor,
      customInstructions: this.customInstructions,
      defaultModel: this.selectedModel,
      files: this.selectedFiles
    });

    this.created.emit(newProject);
    this.closeDialog();
  }

  closeDialog(): void {
    this.closed.emit();
  }

  formatBytes(bytes: number, decimals = 1): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }
}
