import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export type FileContextType = 'Chat' | 'Conversation' | 'Browse';

export interface TrashItem {
  id: string;
  fileName: string;
  location: string;
  fileSize: number;
  context: FileContextType;
  deletedAt: Date;
}

@Component({
  selector: 'app-trash-files-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './trash-files-dialog.html',
  styleUrl: './trash-files-dialog.css',
})
export class TrashFilesDialog {
  @Output() closed = new EventEmitter<void>();

  searchQuery = '';
  selectedContextFilter: 'all' | FileContextType = 'all';
  selectedIds = new Set<string>();

  feedbackMessage: string | null = null;
  feedbackType: 'success' | 'danger' | 'info' = 'info';
  private feedbackTimeout: any = null;

  // Mock UI Data matching all your contexts: Chat, Conversation, and Browse
  trashItems: TrashItem[] = [
    {
      id: 'trash-1',
      fileName: 'rag-vector-knowledge-base.bin',
      location: '/engine-drive/knowledge-base/models/',
      fileSize: 142 * 1024 * 1024, // 142 MB
      context: 'Browse',
      deletedAt: new Date(Date.now() - 1000 * 60 * 60 * 5),
    },
    {
      id: 'trash-2',
      fileName: 'system-architecture-proposal.pdf',
      location: '/chat/prompt-space/attachments/',
      fileSize: 3.4 * 1024 * 1024, // 3.4 MB
      context: 'Chat',
      deletedAt: new Date(Date.now() - 1000 * 60 * 60 * 24),
    },
    {
      id: 'trash-3',
      fileName: 'conversation-history-2026.json',
      location: '/conversations/sessions/archived/',
      fileSize: 840 * 1024, // 840 KB
      context: 'Conversation',
      deletedAt: new Date(Date.now() - 1000 * 60 * 60 * 48),
    },
    {
      id: 'trash-4',
      fileName: 'dataset-training-sample.csv',
      location: '/engine-drive/datasets/sample-v1/',
      fileSize: 45.8 * 1024 * 1024, // 45.8 MB
      context: 'Browse',
      deletedAt: new Date(Date.now() - 1000 * 60 * 60 * 72),
    },
    {
      id: 'trash-5',
      fileName: 'agent-debug-session-transcript.txt',
      location: '/chat/debug-logs/',
      fileSize: 128 * 1024, // 128 KB
      context: 'Chat',
      deletedAt: new Date(Date.now() - 1000 * 60 * 60 * 96),
    },
    {
      id: 'trash-6',
      fileName: 'multi-turn-dialogue-export.md',
      location: '/conversations/exports/',
      fileSize: 512 * 1024, // 512 KB
      context: 'Conversation',
      deletedAt: new Date(Date.now() - 1000 * 60 * 60 * 120),
    }
  ];

  get filteredItems(): TrashItem[] {
    return this.trashItems.filter(item => {
      const matchesSearch =
        !this.searchQuery ||
        item.fileName.toLowerCase().includes(this.searchQuery.toLowerCase().trim()) ||
        item.location.toLowerCase().includes(this.searchQuery.toLowerCase().trim());

      const matchesContext =
        this.selectedContextFilter === 'all' || item.context === this.selectedContextFilter;

      return matchesSearch && matchesContext;
    });
  }

  get totalTrashSizeBytes(): number {
    return this.trashItems.reduce((acc, curr) => acc + curr.fileSize, 0);
  }

  get isAllSelected(): boolean {
    return (
      this.filteredItems.length > 0 &&
      this.filteredItems.every(item => this.selectedIds.has(item.id))
    );
  }

  setContextFilter(filter: 'all' | FileContextType): void {
    this.selectedContextFilter = filter;
  }

  countByContext(context: 'all' | FileContextType): number {
    if (context === 'all') return this.trashItems.length;
    return this.trashItems.filter(i => i.context === context).length;
  }

  toggleSelection(id: string, event?: MouseEvent): void {
    if (event) {
      event.stopPropagation();
    }
    if (this.selectedIds.has(id)) {
      this.selectedIds.delete(id);
    } else {
      this.selectedIds.add(id);
    }
  }

  toggleSelectAll(): void {
    if (this.isAllSelected) {
      this.filteredItems.forEach(i => this.selectedIds.delete(i.id));
    } else {
      this.filteredItems.forEach(i => this.selectedIds.add(i.id));
    }
  }

  // Action: Restore Single File
  restoreFile(file: TrashItem): void {
    this.trashItems = this.trashItems.filter(item => item.id !== file.id);
    this.selectedIds.delete(file.id);
    this.showFeedback(`"${file.fileName}" has been restored to ${file.location}`, 'success');
  }

  // Action: Restore Selected
  restoreSelected(): void {
    const count = this.selectedIds.size;
    this.trashItems = this.trashItems.filter(item => !this.selectedIds.has(item.id));
    this.selectedIds.clear();
    this.showFeedback(`${count} ${count === 1 ? 'file' : 'files'} restored successfully`, 'success');
  }

  // Action: Delete Permanently Single File
  deletePermanently(file: TrashItem): void {
    this.trashItems = this.trashItems.filter(item => item.id !== file.id);
    this.selectedIds.delete(file.id);
    this.showFeedback(`"${file.fileName}" permanently deleted`, 'danger');
  }

  // Action: Delete Selected Permanently
  deletePermanentlySelected(): void {
    const count = this.selectedIds.size;
    this.trashItems = this.trashItems.filter(item => !this.selectedIds.has(item.id));
    this.selectedIds.clear();
    this.showFeedback(`${count} ${count === 1 ? 'file' : 'files'} permanently purged`, 'danger');
  }

  // Action: Empty Trash
  emptyTrash(): void {
    this.trashItems = [];
    this.selectedIds.clear();
    this.showFeedback('Trash repository has been emptied', 'danger');
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

  private showFeedback(message: string, type: 'success' | 'danger' | 'info' = 'info'): void {
    this.feedbackMessage = message;
    this.feedbackType = type;
    if (this.feedbackTimeout) {
      clearTimeout(this.feedbackTimeout);
    }
    this.feedbackTimeout = setTimeout(() => {
      this.feedbackMessage = null;
    }, 3500);
  }
}