import { Component, EventEmitter, OnInit, OnDestroy, Output, ElementRef, ViewChild, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EngineDriveSvg } from '../svgs/engine-drive-svg/engine-drive-svg';
import { concatMap, EMPTY, from, map, Observable, retry } from 'rxjs';
import { form } from '@angular/forms/signals';
import { FileUploadService } from '../../services/file-upload-service';
import { ChunkInitalize, ChunkUpload, InitiateUploadRequest } from '../../services/engine-route-constants';
import { TokenService } from '../../services/token-service';
import { SnackbarService } from '../../services/snackbar-service';

export type ItemCategory = 'folder' | 'model' | 'dataset' | 'document' | 'media' | 'code' | 'archive' | 'other';
export type ViewMode = 'grid' | 'table';
export type SortField = 'name' | 'modifiedAt' | 'size';
export type SortOrder = 'asc' | 'desc';

export interface DriveItem {
  id: string;
  name: string;
  type: 'folder' | 'file';
  category: ItemCategory;
  sizeBytes: number;
  formattedSize: string;
  modifiedAt: Date;
  parentId: string | null;
  extension?: string;
  ragIndexed?: boolean;
  ragStatus?: 'ready' | 'indexing' | 'pending';
  folderColor?: string;
  tags?: string[];
  itemCount?: number;
}

export interface UploadTask {
  id: string;
  fileName: string;
  fileSize: number;
  formattedSize: string;
  uploadedBytes: number;
  progress: number;
  speed: string;
  status: 'uploading' | 'completed' | 'failed' | 'paused';
  category: ItemCategory;
  extension: string;
  targetFolderId: string | null;
}

export interface Breadcrumb {
  id: string | null;
  name: string;
}

export interface StorageCategoryBreakdown {
  category: ItemCategory;
  label: string;
  color: string;
  bytes: number;
  formattedSize: string;
  percent: number;
  itemCount: number;
}

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
  breadcrumbs: Breadcrumb[] = [{ id: null, name: 'Engine Drive' }];

  // Selected item for preview or inspection
  selectedItem: DriveItem | null = null;
  previewItem: DriveItem | null = null;

  // Create folder modal state
  isCreateFolderModalOpen = false;
  newFolderName = '';
  selectedFolderColor = '#00f0ff';
  folderColorOptions = ['#00f0ff', '#0072ff', '#10b981', '#f59e0b', '#ec4899', '#8b5cf6'];
  createFolderError = '';

  // Delete modal state
  isDeleteModalOpen = false;
  itemToDelete: DriveItem | null = null;

  // Upload progress dock
  uploads: UploadTask[] = [];
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

  // Full item dataset
  items: DriveItem[] = [];

  constructor(private cdr: ChangeDetectorRef,
    private uploadService: FileUploadService,
    private tokenService: TokenService,
    private snack: SnackbarService) { }

  ngOnInit(): void {
    this.initializeMockData();
  }

  ngOnDestroy(): void {
    if (this.uploadIntervalId) {
      clearInterval(this.uploadIntervalId);
    }
    if (this.toastTimeout) {
      clearTimeout(this.toastTimeout);
    }
  }

  // ----------------------------------------------------
  // INITIAL MOCK DATA
  // ----------------------------------------------------
  private initializeMockData(): void {
    const now = new Date();
    const hoursAgo = (h: number) => new Date(now.getTime() - h * 60 * 60 * 1000);
    const daysAgo = (d: number) => new Date(now.getTime() - d * 24 * 60 * 60 * 1000);

    this.items = [
      // Root folders
      {
        id: 'folder-rag',
        name: 'RAG Knowledge Base',
        type: 'folder',
        category: 'folder',
        sizeBytes: 0,
        formattedSize: '6 items',
        modifiedAt: hoursAgo(2),
        parentId: null,
        folderColor: '#00f0ff',
        itemCount: 6
      },
      {
        id: 'folder-models',
        name: 'Fine-Tuned Models & Checkpoints',
        type: 'folder',
        category: 'folder',
        sizeBytes: 0,
        formattedSize: '3 models',
        modifiedAt: daysAgo(1),
        parentId: null,
        folderColor: '#0072ff',
        itemCount: 3
      },
      {
        id: 'folder-datasets',
        name: 'Datasets & Benchmarks',
        type: 'folder',
        category: 'folder',
        sizeBytes: 0,
        formattedSize: '4 datasets',
        modifiedAt: daysAgo(3),
        parentId: null,
        folderColor: '#10b981',
        itemCount: 4
      },
      {
        id: 'folder-prompts',
        name: 'System Prompts & Templates',
        type: 'folder',
        category: 'folder',
        sizeBytes: 0,
        formattedSize: '4 files',
        modifiedAt: daysAgo(5),
        parentId: null,
        folderColor: '#f59e0b',
        itemCount: 4
      },

      // Root files
      {
        id: 'file-r1',
        name: 'AIEngine_Architecture_Blueprint.pdf',
        type: 'file',
        category: 'document',
        sizeBytes: 4.8 * 1024 * 1024,
        formattedSize: '4.8 MB',
        modifiedAt: hoursAgo(4),
        parentId: null,
        extension: 'pdf',
        ragIndexed: true,
        ragStatus: 'ready',
        tags: ['Core', 'Specs']
      },
      {
        id: 'file-r2',
        name: 'deepseek_distill_q4_k_m.onnx',
        type: 'file',
        category: 'model',
        sizeBytes: 4.2 * 1024 * 1024 * 1024,
        formattedSize: '4.2 GB',
        modifiedAt: daysAgo(2),
        parentId: null,
        extension: 'onnx',
        ragIndexed: true,
        ragStatus: 'ready',
        tags: ['Production', 'Vision']
      },
      {
        id: 'file-r3',
        name: 'enterprise_intent_embeddings.parquet',
        type: 'file',
        category: 'dataset',
        sizeBytes: 142.6 * 1024 * 1024,
        formattedSize: '142.6 MB',
        modifiedAt: daysAgo(3),
        parentId: null,
        extension: 'parquet',
        ragIndexed: true,
        ragStatus: 'ready',
        tags: ['Vectors', 'HNSW']
      },
      {
        id: 'file-r4',
        name: 'ai_benchmark_multimodal_vision.png',
        type: 'file',
        category: 'media',
        sizeBytes: 2.3 * 1024 * 1024,
        formattedSize: '2.3 MB',
        modifiedAt: daysAgo(4),
        parentId: null,
        extension: 'png',
        ragIndexed: false,
        ragStatus: 'ready',
        tags: ['Chart']
      },
      {
        id: 'file-r5',
        name: 'engine_agent_runtime_config.json',
        type: 'file',
        category: 'code',
        sizeBytes: 84 * 1024,
        formattedSize: '84 KB',
        modifiedAt: hoursAgo(8),
        parentId: null,
        extension: 'json',
        ragIndexed: true,
        ragStatus: 'ready',
        tags: ['Config']
      },

      // Files in RAG Knowledge Base ('folder-rag')
      {
        id: 'file-rag-1',
        name: 'quarterly_financial_audit_2026.pdf',
        type: 'file',
        category: 'document',
        sizeBytes: 7.6 * 1024 * 1024,
        formattedSize: '7.6 MB',
        modifiedAt: hoursAgo(6),
        parentId: 'folder-rag',
        extension: 'pdf',
        ragIndexed: true,
        ragStatus: 'ready',
        tags: ['Finance', 'RAG']
      },
      {
        id: 'file-rag-2',
        name: 'compliance_hipaa_gdpr_handbook.docx',
        type: 'file',
        category: 'document',
        sizeBytes: 3.1 * 1024 * 1024,
        formattedSize: '3.1 MB',
        modifiedAt: daysAgo(1),
        parentId: 'folder-rag',
        extension: 'docx',
        ragIndexed: true,
        ragStatus: 'ready',
        tags: ['Compliance']
      },
      {
        id: 'file-rag-3',
        name: 'enterprise_rag_chunks_manifest.json',
        type: 'file',
        category: 'code',
        sizeBytes: 620 * 1024,
        formattedSize: '620 KB',
        modifiedAt: daysAgo(2),
        parentId: 'folder-rag',
        extension: 'json',
        ragIndexed: true,
        ragStatus: 'ready'
      },
      {
        id: 'file-rag-4',
        name: 'developer_sdk_integration_guide.md',
        type: 'file',
        category: 'document',
        sizeBytes: 340 * 1024,
        formattedSize: '340 KB',
        modifiedAt: daysAgo(3),
        parentId: 'folder-rag',
        extension: 'md',
        ragIndexed: true,
        ragStatus: 'ready'
      },
      {
        id: 'file-rag-5',
        name: 'neural_vector_index_shard_01.bin',
        type: 'file',
        category: 'dataset',
        sizeBytes: 420 * 1024 * 1024,
        formattedSize: '420 MB',
        modifiedAt: daysAgo(4),
        parentId: 'folder-rag',
        extension: 'bin',
        ragIndexed: true,
        ragStatus: 'ready',
        tags: ['Index']
      },
      {
        id: 'file-rag-6',
        name: 'customer_support_telemetry.csv',
        type: 'file',
        category: 'dataset',
        sizeBytes: 18.2 * 1024 * 1024,
        formattedSize: '18.2 MB',
        modifiedAt: daysAgo(5),
        parentId: 'folder-rag',
        extension: 'csv',
        ragIndexed: true,
        ragStatus: 'ready'
      },

      // Files in Fine-Tuned Models ('folder-models')
      {
        id: 'file-mod-1',
        name: 'mistral_7b_engine_instruct_q8.gguf',
        type: 'file',
        category: 'model',
        sizeBytes: 7.8 * 1024 * 1024 * 1024,
        formattedSize: '7.8 GB',
        modifiedAt: daysAgo(1),
        parentId: 'folder-models',
        extension: 'gguf',
        ragIndexed: true,
        ragStatus: 'ready',
        tags: ['GGUF', 'LLM']
      },
      {
        id: 'file-mod-2',
        name: 'coder_agent_lora_adapters.safetensors',
        type: 'file',
        category: 'model',
        sizeBytes: 1.4 * 1024 * 1024 * 1024,
        formattedSize: '1.4 GB',
        modifiedAt: daysAgo(2),
        parentId: 'folder-models',
        extension: 'safetensors',
        ragIndexed: true,
        ragStatus: 'ready',
        tags: ['LoRA']
      },
      {
        id: 'file-mod-3',
        name: 'model_hyperparameters_tuning.json',
        type: 'file',
        category: 'code',
        sizeBytes: 48 * 1024,
        formattedSize: '48 KB',
        modifiedAt: daysAgo(3),
        parentId: 'folder-models',
        extension: 'json',
        ragIndexed: false,
        ragStatus: 'ready'
      },

      // Files in Datasets & Benchmarks ('folder-datasets')
      {
        id: 'file-dat-1',
        name: 'mmlu_pro_reasoning_eval.csv',
        type: 'file',
        category: 'dataset',
        sizeBytes: 34.8 * 1024 * 1024,
        formattedSize: '34.8 MB',
        modifiedAt: daysAgo(3),
        parentId: 'folder-datasets',
        extension: 'csv',
        ragIndexed: true,
        ragStatus: 'ready',
        tags: ['Benchmark']
      },
      {
        id: 'file-dat-2',
        name: 'rlhf_human_preference_pairs.jsonl',
        type: 'file',
        category: 'dataset',
        sizeBytes: 512.4 * 1024 * 1024,
        formattedSize: '512.4 MB',
        modifiedAt: daysAgo(4),
        parentId: 'folder-datasets',
        extension: 'jsonl',
        ragIndexed: true,
        ragStatus: 'ready'
      },
      {
        id: 'file-dat-3',
        name: 'code_synthesis_eval_python.parquet',
        type: 'file',
        category: 'dataset',
        sizeBytes: 88.0 * 1024 * 1024,
        formattedSize: '88 MB',
        modifiedAt: daysAgo(5),
        parentId: 'folder-datasets',
        extension: 'parquet',
        ragIndexed: true,
        ragStatus: 'ready'
      },
      {
        id: 'file-dat-4',
        name: 'gsm8k_math_problem_set.csv',
        type: 'file',
        category: 'dataset',
        sizeBytes: 12.5 * 1024 * 1024,
        formattedSize: '12.5 MB',
        modifiedAt: daysAgo(6),
        parentId: 'folder-datasets',
        extension: 'csv',
        ragIndexed: true,
        ragStatus: 'ready'
      },

      // Files in System Prompts ('folder-prompts')
      {
        id: 'file-prm-1',
        name: 'autonomous_coder_system_prompt.txt',
        type: 'file',
        category: 'document',
        sizeBytes: 24 * 1024,
        formattedSize: '24 KB',
        modifiedAt: daysAgo(5),
        parentId: 'folder-prompts',
        extension: 'txt',
        ragIndexed: true,
        ragStatus: 'ready'
      },
      {
        id: 'file-prm-2',
        name: 'react_agent_reasoning_framework.md',
        type: 'file',
        category: 'document',
        sizeBytes: 38 * 1024,
        formattedSize: '38 KB',
        modifiedAt: daysAgo(6),
        parentId: 'folder-prompts',
        extension: 'md',
        ragIndexed: true,
        ragStatus: 'ready'
      },
      {
        id: 'file-prm-3',
        name: 'voice_synthesis_personas.json',
        type: 'file',
        category: 'code',
        sizeBytes: 96 * 1024,
        formattedSize: '96 KB',
        modifiedAt: daysAgo(7),
        parentId: 'folder-prompts',
        extension: 'json',
        ragIndexed: true,
        ragStatus: 'ready'
      },
      {
        id: 'file-prm-4',
        name: 'structured_json_output_schemas.ts',
        type: 'file',
        category: 'code',
        sizeBytes: 54 * 1024,
        formattedSize: '54 KB',
        modifiedAt: daysAgo(8),
        parentId: 'folder-prompts',
        extension: 'ts',
        ragIndexed: true,
        ragStatus: 'ready'
      }
    ];

    this.updateFolderCounts();
  }

  private updateFolderCounts(): void {
    const folders = this.items.filter(i => i.type === 'folder');
    for (const folder of folders) {
      const count = this.items.filter(i => i.parentId === folder.id).length;
      folder.itemCount = count;
      folder.formattedSize = `${count} ${count === 1 ? 'item' : 'items'}`;
    }
  }

  // ----------------------------------------------------
  // STORAGE CALCULATIONS & BREAKDOWN
  // ----------------------------------------------------
  get totalUsedBytes(): number {
    return this.items
      .filter(i => i.type === 'file')
      .reduce((sum, item) => sum + item.sizeBytes, 0);
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

  get storageBreakdown(): StorageCategoryBreakdown[] {
    const categories: { key: ItemCategory; label: string; color: string }[] = [
      { key: 'model', label: 'AI Models & Weights', color: '#00f0ff' },
      { key: 'dataset', label: 'Datasets & Vectors', color: '#10b981' },
      { key: 'document', label: 'Documents & RAG', color: '#0072ff' },
      { key: 'media', label: 'Media & Images', color: '#f59e0b' },
      { key: 'code', label: 'Code & Configs', color: '#8b5cf6' },
      { key: 'archive', label: 'Archives', color: '#ec4899' },
      { key: 'other', label: 'Other Data', color: '#94a3b8' }
    ];

    const files = this.items.filter(i => i.type === 'file');

    return categories
      .map(cat => {
        const catFiles = files.filter(f => f.category === cat.key);
        const bytes = catFiles.reduce((acc, f) => acc + f.sizeBytes, 0);
        const percent = this.totalStorageBytes > 0 ? (bytes / this.totalStorageBytes) * 100 : 0;
        return {
          category: cat.key,
          label: cat.label,
          color: cat.color,
          bytes,
          formattedSize: this.formatBytes(bytes),
          percent: Math.max(0.5, Math.round(percent * 10) / 10),
          itemCount: catFiles.length
        };
      })
      .filter(item => item.bytes > 0);
  }

  // ----------------------------------------------------
  // FILTERING, SEARCHING & NAVIGATION
  // ----------------------------------------------------
  get displayedFolders(): DriveItem[] {
    if (this.activeCategoryFilter !== 'all' && this.activeCategoryFilter !== 'folder') {
      return [];
    }

    return this.items.filter(item => {
      if (item.type !== 'folder') return false;
      if (item.parentId !== this.currentFolderId) return false;
      if (this.searchQuery.trim()) {
        return item.name.toLowerCase().includes(this.searchQuery.toLowerCase().trim());
      }
      return true;
    }).sort((a, b) => this.sortItems(a, b));
  }

  get displayedFiles(): DriveItem[] {
    return this.items.filter(item => {
      if (item.type !== 'file') return false;
      if (item.parentId !== this.currentFolderId) return false;

      // Category filter
      if (this.activeCategoryFilter !== 'all') {
        if (this.activeCategoryFilter === 'folder') return false;
        if (item.category !== this.activeCategoryFilter) return false;
      }

      // Search filter
      if (this.searchQuery.trim()) {
        const q = this.searchQuery.toLowerCase().trim();
        const matchesName = item.name.toLowerCase().includes(q);
        const matchesExt = item.extension?.toLowerCase().includes(q);
        const matchesTag = item.tags?.some(t => t.toLowerCase().includes(q));
        return matchesName || matchesExt || matchesTag;
      }

      return true;
    }).sort((a, b) => this.sortItems(a, b));
  }

  get totalCurrentItemCount(): number {
    return this.displayedFolders.length + this.displayedFiles.length;
  }

  get currentFolderName(): string {
    if (!this.currentFolderId) return 'Engine Drive';
    const folder = this.items.find(i => i.id === this.currentFolderId);
    return folder ? folder.name : 'Folder';
  }

  private sortItems(a: DriveItem, b: DriveItem): number {
    let comparison = 0;
    switch (this.sortField) {
      case 'name':
        comparison = a.name.localeCompare(b.name, undefined, { numeric: true, sensitivity: 'base' });
        break;
      case 'modifiedAt':
        comparison = a.modifiedAt.getTime() - b.modifiedAt.getTime();
        break;
      case 'size':
        comparison = a.sizeBytes - b.sizeBytes;
        break;
    }
    return this.sortOrder === 'asc' ? comparison : -comparison;
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

  openFolder(folder: DriveItem): void {
    this.currentFolderId = folder.id;
    this.breadcrumbs.push({ id: folder.id, name: folder.name });
    this.selectedItem = null;
    this.searchQuery = '';
    this.cdr.markForCheck();
  }

  navigateToBreadcrumb(index: number): void {
    if (index >= this.breadcrumbs.length - 1) return;
    const target = this.breadcrumbs[index];
    this.currentFolderId = target.id;
    this.breadcrumbs = this.breadcrumbs.slice(0, index + 1);
    this.selectedItem = null;
    this.cdr.markForCheck();
  }

  navigateUp(): void {
    if (this.breadcrumbs.length <= 1) return;
    this.navigateToBreadcrumb(this.breadcrumbs.length - 2);
  }

  // ----------------------------------------------------
  // FOLDER CREATION
  // ----------------------------------------------------
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
    const trimmed = this.newFolderName.trim();
    if (!trimmed) {
      this.createFolderError = 'Please enter a folder name.';
      return;
    }

    const existing = this.items.find(
      i => i.type === 'folder' && i.parentId === this.currentFolderId && i.name.toLowerCase() === trimmed.toLowerCase()
    );
    if (existing) {
      this.createFolderError = 'A folder with this name already exists here.';
      return;
    }

    const newFolder: DriveItem = {
      id: `folder-${Date.now()}`,
      name: trimmed,
      type: 'folder',
      category: 'folder',
      sizeBytes: 0,
      formattedSize: '0 items',
      modifiedAt: new Date(),
      parentId: this.currentFolderId,
      folderColor: this.selectedFolderColor,
      itemCount: 0
    };

    this.items.unshift(newFolder);
    this.updateFolderCounts();
    this.closeCreateFolderModal();
    this.showToast(`Folder "${trimmed}" created successfully`, 'success');
  }

  // ----------------------------------------------------
  // DELETION (FOLDER & FILE)
  // ----------------------------------------------------
  openDeleteModal(item: DriveItem, event?: MouseEvent): void {
    event?.stopPropagation();
    this.itemToDelete = item;
    this.isDeleteModalOpen = true;
    this.cdr.markForCheck();
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
    this.itemToDelete = null;
    this.cdr.markForCheck();
  }

  confirmDelete(): void {
    if (!this.itemToDelete) return;

    const target = this.itemToDelete;
    if (target.type === 'folder') {
      // Recursively delete folder and its children
      const idsToDelete = new Set<string>();
      const collectIds = (folderId: string) => {
        idsToDelete.add(folderId);
        const children = this.items.filter(i => i.parentId === folderId);
        for (const child of children) {
          if (child.type === 'folder') {
            collectIds(child.id);
          } else {
            idsToDelete.add(child.id);
          }
        }
      };

      collectIds(target.id);
      this.items = this.items.filter(i => !idsToDelete.has(i.id));
      this.showToast(`Folder "${target.name}" and contents deleted`, 'info');
    } else {
      // Single file deletion
      this.items = this.items.filter(i => i.id !== target.id);
      this.showToast(`File "${target.name}" deleted from Drive`, 'info');
    }

    this.updateFolderCounts();
    if (this.selectedItem?.id === target.id) {
      this.selectedItem = null;
    }
    if (this.previewItem?.id === target.id) {
      this.previewItem = null;
    }

    this.closeDeleteModal();
  }

  triggerFileInput(): void {
    this.fileInputRef?.nativeElement.click();
  }

  triggerFolderInput(): void {
    this.folderInputRef?.nativeElement.click();
  }

  FileInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const filesArray = Array.from(input.files);
    from(filesArray).pipe(
      concatMap((file: File) => this.uploadPipeLine(file))
    ).subscribe({
      next: (fileName) => {
        this.snack.showSuccessMessage("File uploaded SuccessFully " + fileName);
      },
      error: (err) => {
        this.snack.showErrorMessage("Failed to upload");
      },
      complete: () => {
        this.snack.showSuccessMessage("All File uploaded");
      }
    });
  }

  private uploadPipeLine(file: File): Observable<string> {

    const initiateUploadRequest: InitiateUploadRequest = {
      fileName: file.name,
      fileSize: file.size,
      contentType: file.type,
      sessionId: this.tokenService.ensureSessionId()
    };

    return this.uploadService.initializeUpload(initiateUploadRequest).pipe(

      concatMap((res: ChunkInitalize) => {
        return this.uploadChunksAdaptive(file, res.sessionId, 0, 0, this.MIN_CHUNK_SIZE).pipe(
          concatMap(() =>
            this.uploadService.finalize(this.tokenService.ensureSessionId())
          ),
          map(() => file.name)
        );
      })
    );
  }

  private uploadChunksAdaptive(file: File, sessionId: string, currentByteOffset: number,
    chunkIndex: number, currentChunkSize: number): Observable<any> {
    if (currentByteOffset >= file.size) {
      return EMPTY;
    }
    const endByte = Math.min(currentByteOffset + currentChunkSize, file.size);
    const chunkBlob = file.slice(currentByteOffset, endByte);
    var data: ChunkUpload = {
      sessionId: sessionId,
      chunk: chunkBlob,
      index: chunkIndex
    }
    return this.uploadService.uploadChunk(data).pipe(
      retry(2),
      concatMap((result: any) => {
        const nextChunkSize = this.calculateNextChunkSize(currentChunkSize, result.durationMs);
        return this.uploadChunksAdaptive(file, sessionId, endByte, chunkIndex + 1, nextChunkSize);
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
    this.isUploadDockExpanded = true;

    for (const file of files) {
      const ext = this.extractExtension(file.name);
      const category = this.detectCategory(ext);

      const task: UploadTask = {
        id: `upload-${Date.now()}-${Math.random().toString(36).substring(2, 7)}`,
        fileName: file.name,
        fileSize: file.size,
        formattedSize: this.formatBytes(file.size),
        uploadedBytes: 0,
        progress: 0,
        speed: '3.8 MB/s',
        status: 'uploading',
        category,
        extension: ext,
        targetFolderId: this.currentFolderId
      };

      this.uploads.unshift(task);
    }

    this.ensureUploadSimulationRunning();
    this.showToast(`Started uploading ${files.length} ${files.length === 1 ? 'file' : 'files'}...`, 'info');
    this.cdr.markForCheck();
  }

  private ensureUploadSimulationRunning(): void {
    if (this.uploadIntervalId) return;

    this.uploadIntervalId = setInterval(() => {
      let hasActive = false;
      const stepBytes = 4.2 * 1024 * 1024; // ~4.2 MB per tick
      const speeds = ['4.1 MB/s', '5.4 MB/s', '6.2 MB/s', '3.9 MB/s', '7.0 MB/s'];

      for (const upload of this.uploads) {
        if (upload.status === 'uploading') {
          hasActive = true;

          // Variable speed per file
          const jitter = 0.8 + Math.random() * 0.4;
          const increment = Math.max(stepBytes * jitter, upload.fileSize * 0.08);
          upload.uploadedBytes = Math.min(upload.fileSize, upload.uploadedBytes + increment);
          upload.progress = Math.min(100, Math.round((upload.uploadedBytes / upload.fileSize) * 100));
          upload.speed = speeds[Math.floor(Math.random() * speeds.length)];

          if (upload.progress >= 100) {
            upload.status = 'completed';
            upload.speed = 'Complete';
            this.handleUploadCompleted(upload);
          }
        }
      }

      if (!hasActive) {
        clearInterval(this.uploadIntervalId);
        this.uploadIntervalId = null;
      }

      this.cdr.markForCheck();
    }, 140);
  }

  private handleUploadCompleted(upload: UploadTask): void {
    const newItem: DriveItem = {
      id: `file-up-${Date.now()}-${Math.random().toString(36).substring(2, 6)}`,
      name: upload.fileName,
      type: 'file',
      category: upload.category,
      sizeBytes: upload.fileSize,
      formattedSize: upload.formattedSize,
      modifiedAt: new Date(),
      parentId: upload.targetFolderId,
      extension: upload.extension,
      ragIndexed: true,
      ragStatus: 'ready',
      tags: ['Uploaded']
    };

    this.items.unshift(newItem);
    this.updateFolderCounts();
    this.showToast(`"${upload.fileName}" uploaded successfully`, 'success');
  }

  cancelUpload(taskId: string): void {
    const task = this.uploads.find(u => u.id === taskId);
    if (task) {
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

  // ----------------------------------------------------
  // PREVIEW / INSPECT MODAL
  // ----------------------------------------------------
  openPreview(item: DriveItem, event?: MouseEvent): void {
    event?.stopPropagation();
    this.previewItem = item;
    this.cdr.markForCheck();
  }

  closePreview(): void {
    this.previewItem = null;
    this.cdr.markForCheck();
  }

  selectItem(item: DriveItem, event?: MouseEvent): void {
    event?.stopPropagation();
    this.selectedItem = this.selectedItem?.id === item.id ? null : item;
    this.cdr.markForCheck();
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
