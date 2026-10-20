import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
  nameError = '';
  constructor() { }

  submitCreateProject(): void {
    if (!this.projectName.trim()) {
      this.nameError = 'Project name is required';
      return;
    }
    this.created.emit();
    this.closeDialog();
  }

  closeDialog(): void {
    this.closed.emit();
  }
}
