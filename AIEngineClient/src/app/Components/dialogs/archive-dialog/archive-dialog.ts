import { Component, EventEmitter, Output, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ArchiveSvg } from '../../svgs/archive-svg/archive-svg';
import { ChatsSvg } from '../../svgs/chats-svg/chats-svg';
import { ArchiveChat, EngineRoutes, PaginationResponse } from '../../../services/engine-route-constants';
import { PromptSpaceService } from '../../../services/prompt-space.service';
import { SnackbarService } from '../../../services/snackbar-service';
import { ChatService } from '../../../services/chat-service';
import { Route, Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';

@Component({
    selector: 'app-archive-dialog',
    imports: [FormsModule, ArchiveSvg, ChatsSvg],
    templateUrl: './archive-dialog.html',
    styleUrl: './archive-dialog.css'
})

export class ArchiveDialog implements OnInit {
    @Output() closed = new EventEmitter<void>();
    private searchSubject = new Subject<string>();

    constructor(private promptService: PromptSpaceService, private chatService: ChatService,
        private snack: SnackbarService, private cdr: ChangeDetectorRef, private router: Router) { }

    activeDropdown: string | null = null;
    archivedChatRows: PaginationResponse<ArchiveChat> = {
        Item: [],
        TotalCount: 0,
        Page: 1,
        PageSize: 5
    };
    currentPage = 1;
    pageSize = 5;
    archiveSearchText = '';
    totalPages = 0;
    totalItems = 0;

    ngOnInit(): void {
        this.intializeArchiveSearchEngine();
        this.getArchiveConversation();
    }

    intializeArchiveSearchEngine() {
        this.searchSubject.pipe(
            debounceTime(500),
            distinctUntilChanged(),
            switchMap(searchText => {
                return this.promptService.getArchivedChat(1, this.pageSize, searchText);
            })
        ).subscribe({
            next: (data: any) => {
                this.archivedChatRows = {
                    Item: data.items,
                    TotalCount: data.totalCount,
                    Page: data.page,
                    PageSize: data.pageSize
                };
                this.cdr.detectChanges();
            },
            error: (err: any) => {
                this.snack.showErrorMessage(err.error);
            }
        });
    }

    onSearchChange() {
        this.searchSubject.next(this.archiveSearchText);
    }

    getArchiveConversation() {
        this.promptService.getArchivedChat(this.currentPage, this.pageSize, this.archiveSearchText).subscribe({
            next: (data: any) => {
                this.archivedChatRows = {
                    Item: data.items,
                    TotalCount: data.totalCount,
                    Page: data.page,
                    PageSize: data.pageSize
                };
                this.cdr.detectChanges();
            },
            error: (err: any) => {
                this.snack.showErrorMessage(err.error);
            }
        });
    }

    unarchiveConversation(event: any, conversationId: string) {
        event.stopPropagation();
        this.chatService.updateConversation(conversationId, [{ op: 'replace', path: '/IsArchived', value: false }]).subscribe({
            next: () => {
                this.archivedChatRows = {
                    ...this.archivedChatRows, Item: this.archivedChatRows.Item.filter(c => c.conversationId != conversationId),
                    TotalCount: Math.max(0, this.archivedChatRows.TotalCount - 1)
                };
                this.snack.showInfoMessage("Conversation Unarchive Successfully");
                this.cdr.markForCheck();
            }, error: (err: any) => {
                this.snack.showErrorMessage(err.error);
            }
        })
    }

    OpenArchiveConversation(event: any, conversationId: string) {
        this.router.navigate([EngineRoutes.PromptSpace, conversationId]);
        this.close();
    }

    toggleDropdown(dropdown: string, event: Event): void {
        event.stopPropagation();
        this.activeDropdown = this.activeDropdown === dropdown ? null : dropdown;
    }

    closeDropdowns(): void {
        this.activeDropdown = null;
    }

    selectPageSize(size: number): void {
        this.pageSize = size;
        this.currentPage = 1;
        this.closeDropdowns();
        this.getArchiveConversation();
    }

    get archiveTotalPages() {
        return Math.ceil(this.archivedChatRows.TotalCount / this.pageSize) || 1;
    }

    get displayedChats() {
        return this.archivedChatRows;
    }

    close(): void {
        this.closeDropdowns();
        this.closed.emit();
    }

    previousPage() {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.getArchiveConversation();
        }
    }
    nextPage() {
        if (this.currentPage < this.archiveTotalPages) {
            this.currentPage++;
            this.getArchiveConversation();
        }
    }
}
