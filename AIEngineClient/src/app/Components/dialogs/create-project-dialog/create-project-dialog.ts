import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AIProject } from '../../../models/project';
import { Snackbar } from '../../snackbar/snackbar';
import { SnackbarService } from '../../../services/snackbar-service';

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
  constructor(private snack: SnackbarService) { }

  submitCreateProject(): void {
    if (!this.projectName.trim()) {
      this.nameError = 'Project name is required';
      this.snack.showErrorMessage(this.nameError);
      return;
    }

    const project: AIProject = {
      name: this.projectName,
      description: this.projectDescription.trim()
    };

    this.created.emit(project);
    this.closeDialog();
  }

  closeDialog(): void {
    this.closed.emit();
  }
}
