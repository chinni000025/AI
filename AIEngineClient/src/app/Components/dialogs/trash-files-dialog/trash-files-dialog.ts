import { ChangeDetectorRef, Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EngineDriveItem } from '../../../services/engine-route-constants';
import { FileUploadService } from '../../../services/file-upload-service';
import { SnackbarService } from '../../../services/snackbar-service';

export type FileContextType = 'Chat' | 'Conversation' | 'Browse';


@Component({
  selector: 'app-trash-files-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './trash-files-dialog.html',
  styleUrl: './trash-files-dialog.css',
})
export class TrashFilesDialog implements OnInit {
  @Output() closed = new EventEmitter<void>();

  constructor(private uploadService: FileUploadService,
    private snackBar: SnackbarService, private cdr: ChangeDetectorRef) { }
  ngOnInit(): void {
    this.loadTrashFilesAsync();
  }

  // Mock UI Data matching all your contexts: Chat, Conversation, and Browse
  trashItems: EngineDriveItem[] = [];

  get filteredItems(): any[] {
    return this.trashItems;
  }

  get totalTrashSizeBytes(): number {
    return this.trashItems.reduce((acc, curr) => acc + curr.fileSize, 0);
  }

  get isAllSelected(): boolean {
    return false;
  }

  setContextFilter(filter: 'all' | FileContextType): void {

  }

  countByContext(context: 'all' | FileContextType): number {
    return 1;
  }

  toggleSelection(id: string, event?: MouseEvent): void {

  }

  toggleSelectAll(): void {

  }

  // Action: Restore Single File
  restoreFile(file: any): void {
  }

  // Action: Restore Selected
  restoreSelected(): void {
  }

  // Action: Delete Permanently Single File
  deletePermanently(file: any): void {
  }

  // Action: Delete Selected Permanently
  deletePermanentlySelected(): void {
  }

  // Action: Empty Trash
  emptyTrash(): void {
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

  extractExtension(fileName: string): string {
    const parts = fileName.split('.');
    return parts.length > 1 ? parts.pop()!.toLowerCase() : 'file';
  }

  private loadTrashFilesAsync() {
    this.uploadService.getTrashFiles().subscribe({
      next: (res: any) => {
        this.trashItems = res as EngineDriveItem[];
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.snackBar.showErrorMessage(err.message);
      }
    });
  }
}