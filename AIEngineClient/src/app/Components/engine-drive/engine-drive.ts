import { Component, EventEmitter, OnInit, OnDestroy, Output, ElementRef, ViewChild, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EngineDriveSvg } from '../svgs/engine-drive-svg/engine-drive-svg';
import {
  catchError,
  concatMap,
  defaultIfEmpty,
  EMPTY,
  finalize,
  from,
  map,
  mergeMap,
  Observable,
  retry,
  Subject,
  takeUntil,
  tap
} from 'rxjs';
import { form } from '@angular/forms/signals';
import { FileUploadService } from '../../services/file-upload-service';
import { ChunkInitalize, ChunkUpload, InitiateUploadRequest } from '../../services/engine-route-constants';
import { SnackbarService } from '../../services/snackbar-service';
import { UploadFileTask } from '../../models/snackbar-config';

export type ItemCategory = 'folder' | 'model' | 'dataset' | 'document' | 'media' | 'code' | 'archive' | 'other';
export type ViewMode = 'grid' | 'table';
export type SortField = 'name' | 'modifiedAt' | 'size';
export type SortOrder = 'asc' | 'desc';





@Component({
  selector: 'app-engine-drive',
  standalone: true,
  imports: [CommonModule, FormsModule, EngineDriveSvg],
  templateUrl: './engine-drive.html',
  styleUrl: './engine-drive.css'
})
export class EngineDrive implements OnInit, OnDestroy {
  @Output() closed = new EventEmitter<void>();
  @ViewChild('fileInput') fileInputRef?: ElementRef<HTMLInputElement>;
  @ViewChild('folderInput') folderInputRef?: ElementRef<HTMLInputElement>;

  // Storage Quota: 50 GB
  readonly totalStorageBytes = 50 * 1024 * 1024 * 1024;

  // View state
  viewMode: ViewMode = 'grid';
  isMaximized = false;
  searchQuery = '';
  activeCategoryFilter: 'all' | 'folder' | 'model' | 'dataset' | 'document' | 'media' = 'all';
  sortField: SortField = 'name';
  sortOrder: SortOrder = 'asc';

  // Navigation
  currentFolderId: string | null = null;



  // Create folder modal state
  isCreateFolderModalOpen = false;
  newFolderName = '';
  selectedFolderColor = '#00f0ff';
  folderColorOptions = ['#00f0ff', '#0072ff', '#10b981', '#f59e0b', '#ec4899', '#8b5cf6'];
  createFolderError = '';

  // Delete modal state
  isDeleteModalOpen = false;


  // Upload progress dock
  uploads: UploadFileTask[] = [];
  isUploadDockExpanded = true;
  private uploadIntervalId: any = null;

  // Drag & drop state
  isDragOver = false;

  // Toast notification
  toastMessage: string | null = null;
  toastType: 'success' | 'info' | 'warning' = 'info';


  private toastTimeout: any = null;
  private readonly MIN_CHUNK_SIZE = 64 * 1024; //64kb
  private readonly MAX_CHUNK_SIZE = 8 * 1024 * 1024; // 8mb
  private readonly TARGET_DURATION_MS = 2000;
  private readonly MaxParallelUploads = 3;
  private uploadCancelSubjects = new Map<string, Subject<void>>();


  constructor(private cdr: ChangeDetectorRef,
    private uploadService: FileUploadService,
    private snack: SnackbarService) { }

  ngOnInit(): void {
  }

  ngOnDestroy(): void {
    if (this.uploadIntervalId) {
      clearInterval(this.uploadIntervalId);
    }
    if (this.toastTimeout) {
      clearTimeout(this.toastTimeout);
    }
  }

  private updateFolderCounts(): void {

  }

  get totalUsedBytes(): number {
    return 0;
  }

  get usedPercentage(): number {
    if (this.totalStorageBytes === 0) return 0;
    const pct = (this.totalUsedBytes / this.totalStorageBytes) * 100;
    return Math.min(100, Math.round(pct * 10) / 10);
  }

  get formattedUsedStorage(): string {
    return this.formatBytes(this.totalUsedBytes);
  }

  get formattedTotalStorage(): string {
    return this.formatBytes(this.totalStorageBytes);
  }

  get formattedFreeStorage(): string {
    return this.formatBytes(Math.max(0, this.totalStorageBytes - this.totalUsedBytes));
  }

  get storageBreakdown(): null {
    return null;
  }

  get displayedFolders() {
    return 0;
  }

  get displayedFiles(): null {
    return null;
  }

  get totalCurrentItemCount(): number {
    return 2;
  }

  get currentFolderName(): string {
    return 'folder';
  }

  private sortItems(): number {
    return 0;
  }

  setSort(field: SortField): void {
    if (this.sortField === field) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortOrder = 'asc';
    }
    this.cdr.markForCheck();
  }

  openFolder(): void {
    return;
  }

  navigateToBreadcrumb(index: number): void {
  }

  navigateUp(): void {
  }

  openCreateFolderModal(): void {
    this.newFolderName = '';
    this.selectedFolderColor = '#00f0ff';
    this.createFolderError = '';
    this.isCreateFolderModalOpen = true;
    this.cdr.markForCheck();
  }

  closeCreateFolderModal(): void {
    this.isCreateFolderModalOpen = false;
    this.newFolderName = '';
    this.createFolderError = '';
    this.cdr.markForCheck();
  }

  submitCreateFolder(): void {

  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.cdr.markForCheck();
  }

  confirmDelete(): void {

  }

  triggerFileInput(): void {
    this.fileInputRef?.nativeElement.click();
  }

  triggerFolderInput(): void {
    this.folderInputRef?.nativeElement.click();
  }

  createUploadTask(file: File): UploadFileTask {
    return {
      id: `upload-${Date.now()}-${Math.random().toString(36).substring(2, 7)}`,
      fileName: file.name,
      fileSize: file.size,
      formattedSize: this.formatBytes(file.size),
      uploadedBytes: 0,
      progress: 0,
      speed: '',
      status: 'uploading',
      extension: this.extractExtension(file.name)
    }
  }


  FileInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const filesArray = Array.from(input.files);
    this.isUploadDockExpanded = true;
    const tasks = filesArray.map(file => this.createUploadTask(file));
    this.uploads.unshift(...tasks);
    this.cdr.markForCheck();

    from(filesArray).pipe(
      mergeMap((file: File, i) => this.uploadPipeLine(file, tasks[i]), this.MaxParallelUploads)
    ).subscribe({
      error: (err) => {
        this.snack.showErrorMessage("Failed to upload");
      }
    });
  }

  private uploadPipeLine(file: File, task: UploadFileTask): Observable<string> {
    const initiateUploadRequest: InitiateUploadRequest = {
      fileName: file.name,
      fileSize: file.size,
      contentType: file.type,
    };

    const fileKey = `${file.name}::${file.size}::${file.lastModified}`;
    const cancel$ = new Subject<void>();

    this.uploadCancelSubjects.set(task.id, cancel$);

    return this.uploadService.initializeUpload(initiateUploadRequest).pipe(
      concatMap((res: ChunkInitalize) => {
        this.uploadService.setUploadSessionId(fileKey, res.uploadSessionId);

        return this.uploadChunksAdaptive(file, res.uploadSessionId, 0, 0, this.MIN_CHUNK_SIZE, task).pipe(
          defaultIfEmpty(undefined),
          concatMap(() => this.uploadService.finalize(res.uploadSessionId)),
          tap(() => this.uploadService.removeUploadSessionId(fileKey)),
          map(() => file.name),
        );
      }),
      takeUntil(cancel$),
      tap(() => {
        task.status = 'completed';
        task.progress = 100;
        task.uploadedBytes = task.fileSize;
        this.handleUploadCompleted(task);
      }),
      catchError(err => {
        task.status = 'failed';
        this.cdr.markForCheck();
        return EMPTY; // swallow here so ONE file's failure doesn't kill the other 2 in-flight uploads
      }),
      finalize(() => this.uploadCancelSubjects.delete(task.id))
    );
  }

  private uploadChunksAdaptive(file: File, uploadSessionId: string, currentByteOffset: number,
    chunkIndex: number, currentChunkSize: number, task: UploadFileTask): Observable<any> {

    if (currentByteOffset >= file.size) {
      return EMPTY;
    }

    const endByte = Math.min(currentByteOffset + currentChunkSize, file.size);
    const chunkBlob = file.slice(currentByteOffset, endByte);
    var data: ChunkUpload = {
      sessionId: uploadSessionId,
      chunk: chunkBlob,
      chunkIndex: chunkIndex
    }

    return this.uploadService.uploadChunk(data).pipe(
      retry(2),
      tap((result: any) => {
        task.uploadedBytes = endByte;
        task.progress = Math.min(100, Math.round((endByte / file.size) * 100));
        const bytesThisChunk = endByte - currentByteOffset;
        const seconds = Math.max(result.durationMs, 1) / 1000;
        task.speed = this.formatBytes(bytesThisChunk / seconds) + '/s';
        this.cdr.markForCheck();
      }),
      concatMap((result: any) => {
        const nextChunkSize = this.calculateNextChunkSize(currentChunkSize, result.durationMs);
        return this.uploadChunksAdaptive(file, uploadSessionId, endByte, chunkIndex + 1, nextChunkSize, task);
      })
    );
  }

  private calculateNextChunkSize(currentSize: number, durationMs: number): number {
    if (durationMs <= 0) return Math.min(currentSize * 2, this.MAX_CHUNK_SIZE);
    const factor = this.TARGET_DURATION_MS / durationMs;
    const dampenedFactor = Math.max(0.5, Math.min(1.5, factor));
    let adjustedSize = currentSize * dampenedFactor;
    adjustedSize = Math.max(this.MIN_CHUNK_SIZE, adjustedSize);
    adjustedSize = Math.min(this.MAX_CHUNK_SIZE, adjustedSize);
    return Math.floor(adjustedSize / 1024) * 1024;
  }

  simulateQuickUpload(): void {
    const sampleFiles = [
      { name: 'deepseek_v3_moe_layers.safetensors', size: 1.8 * 1024 * 1024 * 1024, type: 'model' },
      { name: 'enterprise_rag_knowledge_v3.pdf', size: 6.4 * 1024 * 1024, type: 'document' },
      { name: 'benchmark_token_throughput.csv', size: 28.5 * 1024 * 1024, type: 'dataset' }
    ];
    this.startUploadSimulation(sampleFiles);
  }

  startUploadSimulation(files: { name: string; size: number; type: string }[]): void {
  }

  private ensureUploadSimulationRunning(): void {
  }

  private handleUploadCompleted(upload: UploadFileTask): void {

  }

  cancelUpload(taskId: string): void {
    const task = this.uploads.find(u => u.id === taskId);
    if (task) {
      const cancel$ = this.uploadCancelSubjects.get(taskId);
      cancel$?.next();
      cancel$?.complete();
      task.status = 'failed';
      this.uploads = this.uploads.filter(u => u.id !== taskId);
      this.cdr.markForCheck();
    }
  }

  cancelAllUploads(): void {
    this.uploads = [];
    if (this.uploadIntervalId) {
      clearInterval(this.uploadIntervalId);
      this.uploadIntervalId = null;
    }
    this.cdr.markForCheck();
  }

  clearCompletedUploads(): void {
    this.uploads = this.uploads.filter(u => u.status === 'uploading');
    this.cdr.markForCheck();
  }

  get activeUploadsCount(): number {
    return this.uploads.filter(u => u.status === 'uploading').length;
  }

  get overallUploadProgress(): number {
    if (this.uploads.length === 0) return 0;
    const totalBytes = this.uploads.reduce((sum, u) => sum + u.fileSize, 0);
    const uploadedBytes = this.uploads.reduce((sum, u) => sum + u.uploadedBytes, 0);
    if (totalBytes === 0) return 0;
    return Math.min(100, Math.round((uploadedBytes / totalBytes) * 100));
  }

  // ----------------------------------------------------
  // DRAG & DROP
  // ----------------------------------------------------
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

    if (event.dataTransfer && event.dataTransfer.files.length > 0) {
      const files = Array.from(event.dataTransfer.files).map(f => ({
        name: f.name,
        size: f.size || 1024 * 1024 * 4,
        type: f.type
      }));
      this.startUploadSimulation(files);
    }
  }

  openPreview(event?: MouseEvent): void {

  }

  closePreview(): void {

  }


  // ----------------------------------------------------
  // TOAST NOTIFICATION
  // ----------------------------------------------------
  showToast(message: string, type: 'success' | 'info' | 'warning' = 'info'): void {
    this.toastMessage = message;
    this.toastType = type;
    if (this.toastTimeout) {
      clearTimeout(this.toastTimeout);
    }
    this.toastTimeout = setTimeout(() => {
      this.toastMessage = null;
      this.cdr.markForCheck();
    }, 3200);
    this.cdr.markForCheck();
  }

  // ----------------------------------------------------
  // HELPERS & FORMATTING
  // ----------------------------------------------------
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
    return parts.length > 1 ? parts.pop()!.toLowerCase() : '';
  }

  detectCategory(ext: string): ItemCategory {
    const models = ['onnx', 'safetensors', 'gguf', 'pt', 'pth', 'bin', 'ckpt'];
    const datasets = ['csv', 'parquet', 'jsonl', 'tsv', 'arrow'];
    const docs = ['pdf', 'docx', 'doc', 'txt', 'md', 'rtf'];
    const media = ['png', 'jpg', 'jpeg', 'webp', 'svg', 'mp4', 'mp3', 'wav'];
    const code = ['json', 'py', 'ts', 'js', 'html', 'css', 'yaml', 'yml', 'sh'];
    const archives = ['zip', 'tar', 'gz', '7z', 'rar'];

    if (models.includes(ext)) return 'model';
    if (datasets.includes(ext)) return 'dataset';
    if (docs.includes(ext)) return 'document';
    if (media.includes(ext)) return 'media';
    if (code.includes(ext)) return 'code';
    if (archives.includes(ext)) return 'archive';
    return 'other';
  }

  getFileBadgeClass(category: ItemCategory): string {
    switch (category) {
      case 'model': return 'ed-badge--model';
      case 'dataset': return 'ed-badge--dataset';
      case 'document': return 'ed-badge--document';
      case 'media': return 'ed-badge--media';
      case 'code': return 'ed-badge--code';
      case 'archive': return 'ed-badge--archive';
      default: return 'ed-badge--default';
    }
  }

  formatDate(date: Date): string {
    const now = new Date();
    const isToday = date.toDateString() === now.toDateString();
    if (isToday) {
      return 'Today at ' + date.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });
    }
    return date.toLocaleDateString([], { month: 'short', day: 'numeric', year: 'numeric' });
  }

  toggleMaximize(): void {
    this.isMaximized = !this.isMaximized;
    this.cdr.markForCheck();
  }

  close(): void {
    this.closed.emit();
  }
}
