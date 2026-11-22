import { Component, ElementRef, HostListener, ViewChild, AfterViewChecked, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmationDialog } from '../dialogs/confirmation-dialog/confirmation-dialog';
import { Buttons, CatalogModel, CatalogProvider, EngineConstants, EngineRoutes, HubConfiguration, HubEndpoints, WebRTC } from '../../services/engine-route-constants';
import { DialogService } from '../../services/dialog.service';
import { ActivatedRoute, Router } from '@angular/router';
import { SignalRService } from '../../services/signalr-service';
import { ChatMessage, Conversation } from '../../models/snackbar-config';
import { ThemeService } from '../../services/theme.service';
import { LoaderService } from '../../services/loader-service';
import { SnackbarService } from '../../services/snackbar-service';
import { IdentityService } from '../../services/identity-service';
import { finalize, Subject, takeUntil, timestamp } from 'rxjs';
import { RenameSvg } from "../svgs/rename-svg/rename-svg";
import { ArchiveSvg } from "../svgs/archive-svg/archive-svg";
import { TrashSvg } from "../svgs/trash-svg/trash-svg";
import { FavoriteSvg } from "../svgs/favorite-svg/favorite-svg";
import { ChatsSvg } from "../svgs/chats-svg/chats-svg";
import { BrandLogoSvg } from "../svgs/brand-logo-svg/brand-logo-svg";
import { UserSvg } from "../svgs/user-svg/user-svg";
import { MicSvg } from "../svgs/mic-svg/mic-svg";
import { SpeakerSvg } from "../svgs/speaker-svg/speaker-svg";
import { AttachmentSvg } from "../svgs/attachment-svg/attachment-svg";
import { SendSvg } from "../svgs/send-svg/send-svg";
import { ExpandCollapseSvg } from "../svgs/expand-collapse-svg/expand-collapse-svg";
import { SecureConfigSvg } from "../svgs/secure-config-svg/secure-config-svg";
import { PlusSvg } from "../svgs/plus-svg/plus-svg";
import { SettingsSvg } from "../svgs/settings-svg/settings-svg";
import { LogoutSvg } from "../svgs/logout-svg/logout-svg";
import { ThunderSvg } from "../svgs/thunder-svg/thunder-svg";
import { SunSvg } from "../svgs/sun-svg/sun-svg";
import { MoonSvg } from "../svgs/moon-svg/moon-svg";
import { KebabMenuSvg } from "../svgs/kebab-menu-svg/kebab-menu-svg";
import { DropdownSvg } from "../svgs/dropdown-svg/dropdown-svg";
import { EngineDriveSvg } from "../svgs/engine-drive-svg/engine-drive-svg";
import { ChatService } from '../../services/chat-service';
import { ArchiveDialog } from '../dialogs/archive-dialog/archive-dialog';
import { SettingsDialog } from '../dialogs/settings-dialog/settings-dialog';
import { MarkdownModule } from 'ngx-markdown';
import { PromptSpaceService } from '../../services/prompt-space.service';
import { EngineDrive } from '../engine-drive/engine-drive';
import { CreateProjectDialog } from '../dialogs/create-project-dialog/create-project-dialog';
import { AIProject, AIProjectInfo, ProjectConversationInfo } from '../../models/project';
import { TokenService } from '../../services/token-service';
import { ProjectService } from '../../services/project-service';
import { ProjectSvg } from '../svgs/project-svg/project-svg';
import { SttSessionService } from '../../services/STT/stt-session.service';
import { SttMicrophoneService } from '../../services/STT/stt-microphone.service';
import { SttWebRtcService } from '../../services/STT/stt-webrtc.service';
import { SttError, SttSessionState, SttTranscriptEvent } from '../../models/stt.models';
import { SttTranscriptStore } from '../../services/STT/stt-transcript-store';

@Component({
    selector: 'app-prompt-space',
    imports: [FormsModule, RenameSvg,
        ArchiveSvg, TrashSvg, FavoriteSvg, ThunderSvg,
        ChatsSvg, BrandLogoSvg, UserSvg,
        MicSvg, AttachmentSvg, SendSvg, ExpandCollapseSvg,
        PlusSvg, SettingsSvg, LogoutSvg, SunSvg, MoonSvg, KebabMenuSvg, DropdownSvg, EngineDriveSvg, ArchiveDialog, SettingsDialog, MarkdownModule, EngineDrive, CreateProjectDialog, ProjectSvg],
    templateUrl: './prompt-space.html',
    styleUrl: './prompt-space.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PromptSpace implements AfterViewChecked {
    sidebarCollapsed = false;

    /** Active tab in the sidebar: 'chats' | 'saved' | 'projects' */
    activeTab: 'chats' | 'saved' = 'chats';
    /** Active sub-tab inside Project Workspace: 'chats' | 'sources' */
    projectSubTab: 'chats' | 'sources' = 'chats';
    projectPromptText = '';
    isProjectsExpanded = false;
    isRecentsExpanded = false;
    activeConversationId: string | null = null;
    editingConversationId: string | null = null;
    openMenuId: string | null = null;
    menuOpensUp = false;
    favourites = new Set<string>();
    isTyping = false;
    promptText = '';

    isVoiceModalOpen = false;
    speakingMsgId: string | null = null;
    private speakStopTimeout?: any;
    private shouldScrollToBottom = false;
    showScrollToBottom = false;
    copiedMessageId: string | null = null;
    private copiedMessageResetTimeout?: any;
    @ViewChild('messageContainer') private messageContainer!: ElementRef<HTMLDivElement>;
    @ViewChild('promptInput') private promptInput!: ElementRef<HTMLTextAreaElement>;
    showProfileMenu = false;
    showModelMenu = false;
    showPlusMenu = false;
    isArchiveDialogOpen = false;
    isSettingsDialogOpen = false;
    conversations: Conversation[] = [];
    messages: ChatMessage[] = [];
    tempConversationTitle: string = '';
    selectedProvider = '';
    selectedModel = '';
    activeProviderTab = '';
    isProviderView = true;
    private enableTitleEditTimeout?: any;
    private IsEnteredTriggerd = false;
    modelCatalog: CatalogProvider[] = [];
    userName: string | null = null;
    private scrollTimeout?: any;
    isEngineDriveDialogOpen = false;
    isCreateProjectDialogOpen = false;
    projects: AIProject[] = [];
    activeProject: AIProjectInfo | null = null;
    projectInfo: AIProjectInfo[] = [];
    projectConversation: ProjectConversationInfo[] = [];
    projectConversationMap: Record<string, ProjectConversationInfo[] | undefined> = {};
    expandedSidebarProjectId: string | null = null;
    manuallyCollapsedProjectIds = new Set<string>();
    isThinkMode = false;
    isProjectExiting = false;
    private readonly destroy$ = new Subject<void>();
    sttState: SttSessionState = 'idle';
    sttError: SttError | null = null;
    isSttStarting = false;
    sttInterimText = '';
    sttFinalText = '';
    sttConnectionState: RTCPeerConnectionState = 'new';
    activeSttSessionId: string | null = null;
    private sttOperationId = 0;
    private interimStreamTimer: any = null;
    private targetInterimWords: string[] = [];
    private displayedInterimWords: string[] = [];

    constructor(private dialogService: DialogService,
        public themeService: ThemeService,
        private router: Router,
        private signalr: SignalRService,
        private loader: LoaderService,
        private snack: SnackbarService,
        private ids: IdentityService,
        private chatService: ChatService,
        private cdr: ChangeDetectorRef,
        private route: ActivatedRoute,
        private promptSpaceService: PromptSpaceService,
        private tokenService: TokenService,
        private projectService: ProjectService,
        private sttSessionService: SttSessionService,
        private sttMicrophoneService: SttMicrophoneService,
        private sttWebRtcService: SttWebRtcService,
        private sttTranscriptStore: SttTranscriptStore,
    ) { }

    ngOnInit() {
        this.sidebarCollapsed = window.innerWidth < 768;
        this.getModels();
        this.userName = this.getUserName();
        const hugConfiguration: HubConfiguration = {
            hubName: HubEndpoints.NotificationHub,
            requireAuthentication: true,
            queryParams: {
                sessionId: this.tokenService.ensureSessionId(),
                EngineIgnition: this.tokenService.getAccessToken()
            }
        };
        this.signalr.startConnection(hugConfiguration);

        this.signalr.subscribeHub(HubEndpoints.NotificationHub, EngineConstants.ForceLogout).subscribe(() => {
            this.dialogService.open(ConfirmationDialog, {
                message: "Session Terminated",
                subMessage: "Another active session has been detected for your account in AI Engine.\nThis session has been closed to ensure account security.",
                buttons: [Buttons.Ok]
            }).afterClosed().subscribe(
                {
                    next: () => {
                        this.logout();
                    }
                }
            );
        });

        this.route.paramMap.subscribe(params => {
            const conversationId = params.get(EngineConstants.ConversationId);
            if (conversationId && conversationId !== this.activeConversationId) {
                this.loadConversation(conversationId);
            }
        });

        this.subscribeToSttSession();
        this.subscribeToSttTranscript();
        this.subscribeToSttConnection();
        this.subscribeToSignalRTranscript();
    }

    getUserName(): string | null {
        const token = this.tokenService.getAccessToken();

        if (!token) {
            return null;
        }
        const payload = JSON.parse(atob(token.split('.')[1]));
        const userName = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
        return userName || null;
    }

    ngOnDestroy() {
        this.sttOperationId++;
        this.clearInterimStream();
        clearTimeout(this.enableTitleEditTimeout);
        clearTimeout(this.speakStopTimeout);
        clearTimeout(this.copiedMessageResetTimeout);

        if (this.scrollTimeout) {
            clearTimeout(this.scrollTimeout);
            this.scrollTimeout = null;
        }
        this.isEngineDriveDialogOpen = false;
        this.signalr.unsubscribeHub(HubEndpoints.NotificationHub, EngineConstants.ForceLogout);
        this.signalr.unsubscribeHub(HubEndpoints.NotificationHub, EngineConstants.TranscriptReceived);
        this.sttWebRtcService.close();
        this.releaseSttMicrophone();
        this.destroy$.next();
        this.destroy$.complete();
    }

    ngAfterViewChecked(): void {
        if (this.shouldScrollToBottom) {
            this.scrollTimeout = setTimeout(() => {
                this.scrollToBottom();
            }, 300);
            this.shouldScrollToBottom = false;
        }
    }

    onMessageListScroll(): void {
        this.updateScrollToBottomVisibility();
    }

    toggleSidebar(): void {
        this.sidebarCollapsed = !this.sidebarCollapsed;
    }

    setTab(tab: 'chats' | 'saved'): void {
        this.activeTab = tab;
        if (this.isRecentsExpanded) {
            this.loadConversations();
        }
    }

    toggleMenu(event: MouseEvent, conv: Conversation): void {
        event.stopPropagation();
        if (this.openMenuId === conv.conversationId) {
            this.openMenuId = null;
        } else {
            this.openMenuId = conv.conversationId;
            const yPos = event.clientY;
            const windowHeight = window.innerHeight;
            this.menuOpensUp = windowHeight - yPos < 250;
        }
    }

    @HostListener('document:click')
    closeMenu(): void {
        this.openMenuId = null;
        this.showProfileMenu = false;
        this.showModelMenu = false;
        this.showPlusMenu = false;
    }

    toggleModelMenu(event: MouseEvent): void {
        event.stopPropagation();
        this.showPlusMenu = false;
        this.showModelMenu = !this.showModelMenu;
        if (this.showModelMenu) {
            this.isProviderView = true; // Reset to provider view when opening
        }
    }

    selectModel(provider: string, modelId: string): void {
        this.showModelMenu = false;
        if (this.selectedProvider === provider && this.selectedModel === modelId) {
            return;
        }

        if (!this.activeConversationId) {
            this.applySelectedModel(provider, modelId);
            return;
        }

        this.dialogService.open(ConfirmationDialog, {
            message: 'Switch Model',
            subMessage: 'Switching the model in this conversation may result in a context change.',
            iconType: 'warning-svg',
            variant: 'warning',
            buttons: [
                Buttons.Cancel,
                Buttons.Confirm
            ]
        }).afterClosed().subscribe((result: any) => {
            if (result) {
                this.updateConversationModel(provider, modelId);
            }
        });
    }

    setActiveProvider(providerName: string, event: MouseEvent): void {
        event.stopPropagation();
        this.activeProviderTab = providerName;
        this.isProviderView = false; // Move to models view
    }

    goBackToProviders(event: MouseEvent): void {
        event.stopPropagation();
        this.isProviderView = true;
    }

    getModelsForProvider(providerName: string): CatalogModel[] {
        return this.modelCatalog.find(
            (provider) => provider.name.toLowerCase() === providerName.toLowerCase())?.models || [];
    }

    get selectedModelDisplayName(): string {
        return this.getModelsForProvider(this.selectedProvider).find(
            (model) => model.value === this.selectedModel)?.displayName || this.selectedModel;
    }

    togglePlusMenu(event: MouseEvent): void {
        event.stopPropagation();
        this.showModelMenu = false;
        this.showPlusMenu = !this.showPlusMenu;
    }

    handlePlusAction(action: any): void {
        this.showPlusMenu = false;
    }

    shareConversation(event: MouseEvent, conv: Conversation): void {
        event.stopPropagation();
        this.openMenuId = null;
    }

    openArchiveDialog(event?: MouseEvent): void {
        event?.stopPropagation();
        this.openMenuId = null;
        this.showProfileMenu = false;
        this.isArchiveDialogOpen = true;
        this.cdr.markForCheck();
    }

    closeArchiveDialog(): void {
        this.isArchiveDialogOpen = false;
        this.loadConversations();
        this.cdr.markForCheck();
    }

    openSettingsDialog(event?: MouseEvent): void {
        event?.stopPropagation();
        this.showProfileMenu = false;
        this.isSettingsDialogOpen = true;
        this.cdr.markForCheck();
    }

    closeSettingsDialog(): void {
        this.isSettingsDialogOpen = false;
        this.cdr.markForCheck();
    }

    toggleFavourite(event: MouseEvent, conv: Conversation): void {
        event.stopPropagation();
        this.openMenuId = null;
        this.toggleFavouriteConversation(conv);
    }

    deleteConversation(event: MouseEvent, conv: Conversation): void {
        event.stopPropagation();
        this.openMenuId = null;

        this.dialogService.open(ConfirmationDialog, {
            message: 'Delete Conversation',
            subMessage: `"${conv.conversationTitle}" will be moved to the Engine Recycle Bin.`,
            iconType: 'trash-svg',
            variant: 'danger',
            buttons: [Buttons.Cancel, Buttons.Delete]
        }).afterClosed().subscribe((result: any) => {
            if (result) {
                this.chatService.deleteConversation(conv.conversationId).subscribe({
                    next: (res) => {
                        this.conversations = this.conversations.filter(c => c.conversationId !== conv.conversationId);
                        this.projects.forEach(p => {
                        });
                        this.cdr.detectChanges();
                        this.snack.showInfoMessage("Conversation Is Moved To Recylce Bin");
                        if (this.activeConversationId === conv.conversationId) {
                            this.activeConversationId = null;
                            this.router.navigate([EngineRoutes.PromptSpace]);
                        }
                    }, error: (err) => {
                        this.snack.showErrorMessage(err.error);
                    }
                });
            }
        });
    }

    startNewChat(): void {
        this.router.navigate([EngineRoutes.PromptSpace]);
        this.activeConversationId = null;
        this.activeProject = null;
        this.messages = [];
        this.promptText = '';
        this.projectPromptText = '';
        this.shouldScrollToBottom = true;
        this.cdr.markForCheck();
    }

    loadConversation(conversationId: string, projectContext?: AIProjectInfo | null): void {
        this.activeConversationId = conversationId;
        if (projectContext !== undefined) {
            this.activeProject = projectContext;
        }
        // Keep the sidebar project expanded and highlighted when opening a project conversation
        if (projectContext) {
            this.expandedSidebarProjectId = projectContext.id;
            this.isProjectsExpanded = true;
            // Load conversations for this project into map if not already loaded
            if (!this.projectConversationMap[projectContext.id]) {
                this.getProjectConversation(projectContext.id);
            }
        }
        this.router.navigate([EngineRoutes.PromptSpace, this.activeConversationId]);
        this.chatService.getConversationsById(conversationId).subscribe({
            next: (data: any) => {
                this.messages = data.messages.map((m: any) => ({
                    id: m.messageId?.toString(),
                    role: m.roleId == 1 ? EngineConstants.User : EngineConstants.Assistant,
                    content: m.content,
                    timestamp: m.messagSentAt
                }));
                this.applyConversationModel(data.modelUsed);
                this.cdr.markForCheck();
                this.shouldScrollToBottom = true;
            }
        });
    }

    onEnterKey(event: Event): void {
        const ke = event as KeyboardEvent;
        if (!ke.shiftKey) {
            ke.preventDefault();
            this.sendMessage();
        }
    }

    onInputChange(): void {
        const el = this.promptInput?.nativeElement;
        if (el) {
            el.style.height = 'auto';
            el.style.height = Math.min(el.scrollHeight, 160) + 'px';
        }
    }

    openVoiceModal(): void {
        this.sttTranscriptStore.reset();
        this.resetSttTranscript();
        this.isVoiceModalOpen = true;
        this.cdr.markForCheck();
        this.startSttSession();
    }

    closeVoiceModal(): void {
        this.sttOperationId++;
        this.isVoiceModalOpen = false;
        this.stopSttSession();
        this.resetSttTranscript();
        this.sttTranscriptStore.reset();
        this.sttSessionService.reset();
        this.cdr.markForCheck();
    }

    async toggleSttRecording(): Promise<void> {
        if (this.sttState === 'listening' || this.sttState === 'connecting') {
            this.stopSttSession();
        } else if (this.sttState === 'idle' || this.sttState === 'error') {
            await this.startSttSession();
        }
    }

    useVoiceTranscript(): void {
        const transcript = [this.sttFinalText.trim(), this.sttInterimText.trim()]
            .filter(Boolean)
            .join(' ');
        if (!transcript) {
            return;
        }

        this.promptText = this.appendTranscript(this.promptText, transcript);

        this.stopSttSession();
        this.closeVoiceModal();
        this.cdr.markForCheck();
    }

    private appendTranscript(existingText: string, transcript: string): string {
        const existing = existingText ?? '';
        const recognized = transcript.trim();

        if (!recognized) {
            return existing;
        }

        if (!existing.trim()) {
            return recognized;
        }

        const separator = /\s$/.test(existing) ? '' : ' ';
        return `${existing}${separator}${recognized}`;
    }

    private subscribeToSttSession(): void {
        this.sttSessionService.state$
            .pipe(takeUntil(this.destroy$))
            .subscribe(state => {
                this.sttState = state.state;
                this.sttError = state.error;

                this.cdr.markForCheck();
            });
    }

    private releaseSttMicrophone(): void {
        this.sttMicrophoneService.release();
    }

    private subscribeToSttTranscript(): void {
        this.sttTranscriptStore.state$
            .pipe(takeUntil(this.destroy$))
            .subscribe(state => {
                const newFinal = state.finalizedUtterances.join(' ');
                if (newFinal !== this.sttFinalText) {
                    this.sttFinalText = newFinal;
                }

                this.streamInterimLively(state.interimText);
                this.cdr.markForCheck();
            });
    }

    private streamInterimLively(targetText: string): void {
        const clean = (targetText || '').trim();
        if (!clean) {
            this.clearInterimStream();
            this.sttInterimText = '';
            this.displayedInterimWords = [];
            this.targetInterimWords = [];
            this.cdr.markForCheck();
            return;
        }

        const words = clean.split(/\s+/).filter(Boolean);
        this.targetInterimWords = words;

        // If displayed text is already ahead or diverged from target, synchronize base
        if (this.displayedInterimWords.length > words.length ||
            (this.displayedInterimWords.length > 0 && this.displayedInterimWords[0].toLowerCase() !== words[0].toLowerCase())) {
            this.displayedInterimWords = words.slice(0, Math.max(0, words.length - 2));
            this.sttInterimText = this.displayedInterimWords.join(' ');
        }

        if (!this.interimStreamTimer) {
            this.tickStreamWord();
        }
    }

    private tickStreamWord(): void {
        this.clearInterimStream();

        const step = () => {
            if (this.displayedInterimWords.length < this.targetInterimWords.length) {
                const nextIdx = this.displayedInterimWords.length;
                this.displayedInterimWords.push(this.targetInterimWords[nextIdx]);
                this.sttInterimText = this.displayedInterimWords.join(' ');
                this.cdr.markForCheck();

                if (this.displayedInterimWords.length < this.targetInterimWords.length) {
                    const lag = this.targetInterimWords.length - this.displayedInterimWords.length;
                    const delay = lag > 4 ? 25 : 55;
                    this.interimStreamTimer = setTimeout(step, delay);
                } else {
                    this.interimStreamTimer = null;
                }
            } else {
                this.interimStreamTimer = null;
            }
        };

        step();
    }

    private clearInterimStream(): void {
        if (this.interimStreamTimer) {
            clearTimeout(this.interimStreamTimer);
            this.interimStreamTimer = null;
        }
    }

    private resetSttTranscript(): void {
        this.clearInterimStream();
        this.displayedInterimWords = [];
        this.targetInterimWords = [];
        this.sttInterimText = '';
        this.sttFinalText = '';
    }

    get hasSttTranscript(): boolean {
        return !!(this.sttInterimText.trim() || this.sttFinalText.trim());
    }

    private generateGuid(): string {
        if (typeof crypto !== 'undefined' && crypto.randomUUID) {
            return crypto.randomUUID();
        }
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = Math.random() * 16 | 0;
            const v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    private cleanupSttAudio(): void {
        this.sttWebRtcService.close();
        this.sttMicrophoneService.release();
    }

    async startSttSession(): Promise<void> {
        if (this.isSttStarting) {
            return;
        }

        if (!this.sttSessionService.begin()) {
            return;
        }

        const operationId = ++this.sttOperationId;
        this.isSttStarting = true;
        try {
            const stream = await this.sttMicrophoneService.acquire();

            if (operationId !== this.sttOperationId) {
                this.sttMicrophoneService.release();
                return;
            }

            this.sttSessionService.markConnecting();
            this.cdr.markForCheck();

            // WebRTC initialization and backend signaling
            const rtcConfig: RTCConfiguration = {
                iceServers: [{ urls: WebRTC.STUNServer }]
            };
            this.sttWebRtcService.initialize(stream, rtcConfig);

            const activeSessionId = this.generateGuid();
            this.activeSttSessionId = activeSessionId;
            this.sttTranscriptStore.startSession(activeSessionId);

            // Forward ICE candidates to backend
            this.sttWebRtcService.iceCandidate$
                .pipe(takeUntil(this.destroy$))
                .subscribe(candidate => {
                    if (candidate && candidate.candidate && this.activeSttSessionId) {
                        this.chatService.sendSttIceCandidate(
                            this.activeSttSessionId,
                            candidate.candidate,
                            candidate.sdpMid,
                            candidate.sdpMLineIndex
                        ).subscribe({ error: () => { } });
                    }
                });

            const offer = await this.sttWebRtcService.createOffer();
            const connectionId = this.signalr.getConnectionId(HubEndpoints.NotificationHub) ?? this.tokenService.ensureSessionId();
            this.chatService.sendSttOffer(activeSessionId, offer.sdp ?? '', connectionId)
                .pipe(takeUntil(this.destroy$))
                .subscribe({
                    next: async (res: any) => {
                        if (operationId !== this.sttOperationId) {
                            return;
                        }
                        if (res?.sdpAnswer) {
                            await this.sttWebRtcService.setRemoteDescription({
                                type: 'answer',
                                sdp: res.sdpAnswer
                            });
                            this.sttSessionService.markListening();
                            this.cdr.markForCheck();
                        }
                    },
                    error: (err: any) => {
                        if (operationId !== this.sttOperationId) return;
                        this.cleanupSttAudio();
                        this.sttSessionService.fail({
                            code: 'WEBRTC_SIGNALING_FAILED',
                            message: err?.error?.error || err?.message || 'Failed to negotiate audio connection with server.',
                            recoverable: true
                        });
                        this.cdr.markForCheck();
                    }
                });
        } catch (error) {
            if (operationId !== this.sttOperationId) {
                return;
            }
            this.cleanupSttAudio();
            this.sttSessionService.fail({
                code: 'MICROPHONE_ACCESS_FAILED',
                message: error instanceof Error
                    ? error.message
                    : 'Unable to access the microphone.',
                recoverable: true
            });
        } finally {
            this.isSttStarting = false;
            this.cdr.markForCheck();
        }
    }

    stopSttSession(): void {
        this.sttOperationId++;
        const currentState = this.sttSessionService.snapshot.state;

        if (this.activeSttSessionId) {
            const closingSessionId = this.activeSttSessionId;
            this.activeSttSessionId = null;
            this.chatService.stopSttBackendSession(closingSessionId).subscribe({ error: () => { } });
        }

        if (currentState === 'idle') {
            return;
        }

        if (currentState !== 'error') {
            this.sttSessionService.markStopping();
        }

        this.cleanupSttAudio();

        if (currentState !== 'error') {
            this.sttSessionService.reset();
        }

        this.cdr.markForCheck();
    }

    cancelSttSession(): void {
        this.stopSttSession();
        this.resetSttTranscript();
        this.closeVoiceModal();
    }

    private subscribeToSttConnection(): void {
        this.sttWebRtcService.connectionState$
            .pipe(takeUntil(this.destroy$))
            .subscribe(connectionState => {
                this.sttConnectionState = connectionState;

                this.handleSttConnectionState(connectionState);

                this.cdr.markForCheck();
            });
    }

    private handleSttConnectionState(connectionState: RTCPeerConnectionState): void {
        const currentState = this.sttSessionService.snapshot.state;
        switch (connectionState) {
            case 'connected':
                if (currentState === 'connecting' || currentState === 'reconnecting') {
                    this.sttSessionService.markListening();
                }
                break;

            case 'disconnected':
                if (currentState === 'connecting' || currentState === 'listening') {
                    this.sttSessionService.markReconnecting();
                }
                break;

            case 'failed':
                if (currentState !== 'idle' && currentState !== 'stopping' && currentState !== 'error') {
                    this.sttSessionService.fail({
                        code: 'WEBRTC_CONNECTION_FAILED',
                        message: 'The audio connection could not be established. Please try again.',
                        recoverable: true
                    });

                    this.sttWebRtcService.close();
                    this.sttMicrophoneService.release();
                }
                break;

            case 'new':
            case 'connecting':
            case 'closed':
                // No state transition is required here.
                break;
        }
    }

    private handleSttTranscriptEvent(event: any): void {
        if (!event) {
            return;
        }
        const accepted = this.sttTranscriptStore.applyTranscript(event);
        if (!accepted) {
            return;
        }

        this.cdr.markForCheck();
    }

    private subscribeToSignalRTranscript(): void {
        this.signalr.subscribeHub<SttTranscriptEvent>(HubEndpoints.NotificationHub, EngineConstants.TranscriptReceived)
            .pipe(takeUntil(this.destroy$))
            .subscribe(event => {
                if (event) {
                    this.handleSttTranscriptEvent(event);
                }
            });
    }

    get sttStatusLabel(): string {
        switch (this.sttState) {
            case 'requesting-permission':
                return 'Requesting microphone access…';
            case 'connecting':
                return 'Connecting live audio stream…';
            case 'listening':
                return 'Listening… speak naturally';
            case 'reconnecting':
                return 'Reconnecting audio stream…';
            case 'stopping':
                return 'Finalizing transcription…';
            case 'error':
                return this.sttError?.message || 'Speech recognition encountered an error';
            case 'idle':
            default:
                return this.hasSttTranscript ? 'Transcription ready' : 'Tap the microphone to speak';
        }
    }

    onVoiceInput(): void {
        this.openVoiceModal();
    }

    async copyMessage(msg: ChatMessage, event: MouseEvent): Promise<void> {
        event.stopPropagation();

        const text = (msg.content ?? '').toString();
        if (!text.trim()) {
            return;
        }

        clearTimeout(this.copiedMessageResetTimeout);
        this.copiedMessageId = msg.id;
        this.cdr.detectChanges();

        try {
            await this.writeToClipboard(text);
            this.snack.showSuccessMessage('Copied to clipboard');
            this.copiedMessageResetTimeout = setTimeout(() => {
                if (this.copiedMessageId === msg.id) {
                    this.copiedMessageId = null;
                    this.cdr.detectChanges();
                }
            }, 1800);
        } catch (err) {
            console.error('Unable to copy message:', err);
            if (this.copiedMessageId === msg.id) {
                this.copiedMessageId = null;
                this.cdr.detectChanges();
            }
            this.snack.showErrorMessage('Unable to copy message');
        }
    }

    private async writeToClipboard(text: string): Promise<void> {
        if (navigator.clipboard?.writeText) {
            await navigator.clipboard.writeText(text);
            return;
        }

        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.setAttribute('readonly', '');
        textarea.style.position = 'fixed';
        textarea.style.left = '-9999px';
        document.body.appendChild(textarea);
        textarea.focus();
        textarea.select();
        textarea.setSelectionRange(0, textarea.value.length);

        try {
            const copied = document.execCommand('copy');
            if (!copied) {
                throw new Error('Copy command was rejected.');
            }
        } finally {
            document.body.removeChild(textarea);
        }
    }

    loadConversations() {
        if (this.activeTab === 'chats') {
            this.chatService.getAllConversations().subscribe(
                {
                    next: (res: any) => {
                        this.conversations = res;
                        this.cdr.markForCheck();
                    },
                    error: (err) => {
                        console.log(err);
                    }
                }
            );
        } else {
            this.chatService.getFavouriteConversations().subscribe(
                {
                    next: (res: any) => {
                        this.conversations = res;
                        this.cdr.markForCheck();
                    },
                    error: (err) => {
                        console.log(err);
                    }
                }
            )
        }
    }

    sendMessage(projectId?: string): void {
        const text = this.promptText.trim();
        if (!text) {
            return;
        }

        const userMsg: ChatMessage = {
            id: this.generateId(),
            role: EngineConstants.User,
            content: text,
            timestamp: this.getCurrentTime(),
        };
        this.messages = [...this.messages, userMsg];
        this.promptText = '';

        if (this.promptInput?.nativeElement) {
            this.promptInput.nativeElement.style.height = 'auto';
        }

        this.shouldScrollToBottom = true;
        this.isTyping = true;
        let promptToSend = text;
        const payload = {
            Prompt: promptToSend,
            Model: this.selectedModel,
            Provider: this.selectedProvider,
            ProjectId: projectId || this.activeProject?.id
        }

        const Request$ = this.activeConversationId ? this.chatService.sendMessage(this.activeConversationId, payload) :
            this.chatService.sendNewMessage(payload);

        Request$.pipe(finalize(() => {
            this.isTyping = false;
            this.cdr.markForCheck();
            this.shouldScrollToBottom = true;
        })).subscribe({
            next: (res: any) => {
                if (res.response.isNewConversation) {
                    this.activeConversationId = res.response.conversationId;
                    this.loadConversations(); // load side bar.
                    if (projectId || this.activeProject?.id) {
                        this.getProjectConversation(projectId ?? this.activeProject?.id);
                    }
                    this.router.navigate([EngineRoutes.PromptSpace, this.activeConversationId]);
                }
                const aiMsg: ChatMessage = {
                    id: this.generateId(),
                    role: EngineConstants.Assistant,
                    content: res.response?.output,
                    timestamp: this.getCurrentTime(),
                };
                this.messages = [...this.messages, aiMsg];
                this.cdr.markForCheck();
                this.shouldScrollToBottom = true;
            },
            error: (err) => {
                // Optionally show an error message in the chat
                const errorMsg: ChatMessage = {
                    id: this.generateId(),
                    role: EngineConstants.Assistant,
                    content: 'Sorry, I encountered an error. Please try again.',
                    timestamp: this.getCurrentTime(),
                };
                this.messages = [...this.messages, errorMsg];
            }
        });
    }

    scrollToBottom(): void {
        try {
            const el = this.messageContainer?.nativeElement;
            if (el) {
                el.scrollTo({ top: el.scrollHeight, behavior: 'smooth' });
                this.showScrollToBottom = false;
                this.cdr.markForCheck();
            }
        } catch { }
    }

    private updateScrollToBottomVisibility(): void {
        const el = this.messageContainer?.nativeElement;
        if (!el) {
            return;
        }

        const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight;
        const shouldShow = distanceFromBottom > 140;

        if (this.showScrollToBottom !== shouldShow) {
            this.showScrollToBottom = shouldShow;
            this.cdr.markForCheck();
        }
    }

    private generateId(): string {
        return `msg-${Date.now()}-${Math.floor(Math.random() * 9999)}`;
    }

    private getCurrentTime(): string {
        return new Date().toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit',
        });
    }

    toggleProfileMenu(event: MouseEvent): void {
        event.stopPropagation();
        this.showProfileMenu = !this.showProfileMenu;
    }

    logout() {
        this.loader.show("Logout from the Engine");
        this.ids.logout().pipe(finalize(() => { this.loader.hide() })).subscribe({
            next: () => {
                this.snack.showSuccessMessage("Log out From the AI Engine");
                this.router.navigate(['']);
            },
            error: (err) => {
                this.snack.showErrorMessage(err.error);
            }
        });
    }

    enableTitleEdit(conv: Conversation): void {
        this.editingConversationId = conv.conversationId;
        this.tempConversationTitle = conv.conversationTitle;
        this.cdr.markForCheck();

        clearTimeout(this.enableTitleEditTimeout);
        this.enableTitleEditTimeout = setTimeout(() => {
            const editInput = document.querySelector<HTMLInputElement>(
                `input[data-conversation-id="${conv.conversationId}"]`
            );
            editInput?.focus();
        });
    }

    renameConversation(event: MouseEvent, conv: Conversation): void {
        event.stopPropagation();
        this.openMenuId = null;
        this.enableTitleEdit(conv);
    }

    onEnter(event: any, conv: Conversation) {
        event.preventDefault();
        this.IsEnteredTriggerd = true;
        this.saveTitle(conv);
        (event.target as HTMLInputElement).blur();
    }

    onBlur(conv: Conversation) {
        if (this.IsEnteredTriggerd) {
            this.IsEnteredTriggerd = false;
            return;
        }
        this.saveTitle(conv);
    }

    saveTitle(conv: Conversation): void {
        const trimmedTitle = this.tempConversationTitle.trim();

        if (trimmedTitle) {
            conv.conversationTitle = trimmedTitle;
            this.chatService.updateConversation(conv.conversationId,
                [{ op: 'replace', path: '/Title', value: conv.conversationTitle }]).subscribe({
                    next: (res) => {
                        this.snack.showSuccessMessage("Title Updated Successfully");
                    },
                    error: (err) => {
                        console.log(err);
                        this.snack.showErrorMessage(err.error);
                    }
                });
        }

        this.editingConversationId = null;
    }

    toggleFavouriteConversation(conv: Conversation): void {
        this.chatService.updateConversation(conv.conversationId,
            [{ op: 'replace', path: '/IsFavorite', value: !conv.isFavorite }]).pipe(
                finalize(() => this.loadConversations())
            ).subscribe({
                next: (res) => {
                    if (conv.isFavorite) {
                        this.snack.showSuccessMessage("Conversation Removed from Favourites");
                    } else {
                        this.snack.showSuccessMessage("Conversation Moved to Favourites");
                    }
                },
                error: (err) => {
                    console.log(err);
                    this.snack.showErrorMessage(err.error);
                }
            });
    }

    archiveConversation(event: any, conversation: Conversation) {
        event.stopPropagation();
        this.chatService.updateConversation(conversation.conversationId, [{ op: 'replace', path: '/IsArchived', value: true }]).subscribe({
            next: () => {
                this.conversations = this.conversations.filter(c => c.conversationId != conversation.conversationId);
                if (this.activeConversationId === conversation.conversationId) {
                    this.startNewChat();
                }
                this.snack.showInfoMessage("Conversation Archive Successfully");
                this.cdr.markForCheck();
            }, error: (err) => {
                this.snack.showErrorMessage(err.error);
            }
        })
    }

    cancelEdit(): void {
        this.editingConversationId = null;
        this.cdr.markForCheck();
    }

    getModels() {
        this.promptSpaceService.getModels().subscribe((models: CatalogProvider[]) => {
            this.modelCatalog = models ?? [];
            this.selectedProvider = this.modelCatalog[0]?.name ?? '';
            this.selectedModel = this.modelCatalog[0]?.models[0]?.value ?? '';
            this.activeProviderTab = this.selectedProvider;

            this.cdr.markForCheck();
        });
    }

    private updateConversationModel(provider: string, modelId: string): void {
        if (!this.activeConversationId) {
            this.applySelectedModel(provider, modelId);
            return;
        }

        this.chatService.updateConversation(this.activeConversationId,
            [{ op: 'replace', path: '/ModelUsed', value: modelId }]).subscribe({
                next: () => {
                    this.applySelectedModel(provider, modelId);
                },
                error: (err) => {
                    console.log(err);
                    this.snack.showErrorMessage(err.error);
                }
            });
    }

    private applySelectedModel(provider: string, modelId: string): void {
        this.selectedProvider = provider;
        this.selectedModel = modelId;
        this.activeProviderTab = provider;
        this.cdr.markForCheck();
    }

    private applyConversationModel(modelId?: string | null, providerName?: string | null): void {
        if (!modelId) {
            return;
        }

        const provider = providerName
            || this.findProviderByModel(modelId)?.name
            || this.selectedProvider

        this.applySelectedModel(provider, modelId);
    }

    private findProviderByModel(modelId: string): CatalogProvider | undefined {
        return this.modelCatalog.find((provider) =>
            provider.models.some((model) => model.value.toLowerCase() === modelId.toLowerCase())
        );
    }

    formatTimestamp(timestamp: string): string {
        const date = new Date(timestamp);

        if (isNaN(date.getTime())) {
            return timestamp;
        }

        const now = new Date();

        const isToday =
            date.getDate() === now.getDate() &&
            date.getMonth() === now.getMonth() &&
            date.getFullYear() === now.getFullYear();

        if (isToday) {
            return date.toLocaleTimeString([], {
                hour: 'numeric',
                minute: '2-digit'
            });
        }

        return date.toLocaleDateString([], {
            month: 'short',
            day: 'numeric'
        }) +
            ' ' +
            date.toLocaleTimeString([], {
                hour: 'numeric',
                minute: '2-digit'
            });
    }

    openEngineDrive(event?: MouseEvent): void {
        event?.stopPropagation();
        this.showProfileMenu = false;
        this.isEngineDriveDialogOpen = true;
        this.cdr.markForCheck();
    }

    closeEngineDriveDialog(): void {
        this.isEngineDriveDialogOpen = false;
        this.cdr.markForCheck();
    }

    toggleProjectsSection(): void {
        this.isProjectsExpanded = !this.isProjectsExpanded;
        if (this.isProjectsExpanded) {
            this.getProjects();
        }
        this.cdr.markForCheck();
    }

    toggleRecentsSection(): void {
        this.isRecentsExpanded = !this.isRecentsExpanded;
        if (this.isRecentsExpanded) {
            this.loadConversations();
        }
        this.cdr.markForCheck();
    }

    openCreateProjectDialog(event?: MouseEvent): void {
        event?.stopPropagation();
        this.isCreateProjectDialogOpen = true;
        this.cdr.markForCheck();
    }

    closeCreateProjectDialog(): void {
        this.isCreateProjectDialogOpen = false;
        this.cdr.markForCheck();
    }

    onProjectCreated(project: AIProject): void {
        this.projectService.createProject(project).subscribe({
            next: (res) => {
                this.snack.showSuccessMessage("Project Created Successfully");
                this.getProjects();
            }, error: (err) => {
                this.snack.showErrorMessage(err);
            }
        });
    }

    getProjects(): void {
        this.projectService.getProjects().subscribe({
            next: (res: any) => {
                this.projectInfo = res as AIProjectInfo[];
                this.cdr.markForCheck();
            }, error: (err) => {
                this.snack.showErrorMessage(err);
            }
        });
    }

    isProjectExpanded(proj: AIProjectInfo): boolean {
        if (!proj) return false;
        if (this.manuallyCollapsedProjectIds.has(proj.id)) {
            return false;
        }
        return this.expandedSidebarProjectId === proj.id || this.activeProject?.id === proj.id;
    }

    toggleProjectChevron(proj: AIProjectInfo, event: Event): void {
        event.stopPropagation();
        if (this.isProjectExpanded(proj)) {
            this.manuallyCollapsedProjectIds.add(proj.id);
            if (this.expandedSidebarProjectId === proj.id) {
                this.expandedSidebarProjectId = null;
            }
        } else {
            this.manuallyCollapsedProjectIds.delete(proj.id);
            this.expandedSidebarProjectId = proj.id;
            if (!this.projectConversationMap[proj.id]) {
                this.getProjectConversation(proj.id);
            }
        }
        this.cdr.markForCheck();
    }

    openProjectWorkspace(project: AIProjectInfo, event?: Event): void {
        event?.stopPropagation();
        this.expandedSidebarProjectId = this.expandedSidebarProjectId === project.id ? null : project.id;
        this.isProjectExiting = false;
        this.activeProject = project;
        this.activeConversationId = null;
        this.messages = [];
        this.projectSubTab = 'chats';
        this.projectPromptText = '';
        this.getProjectConversation(project.id);
        this.router.navigate([EngineRoutes.PromptSpace]);
        this.cdr.markForCheck();
    }

    sendProjectMessage(): void {
        const text = this.projectPromptText.trim();
        if (!text) return;
        this.isProjectExiting = true;
        this.cdr.markForCheck();

        setTimeout(() => {
            this.promptText = text;
            this.projectPromptText = '';
            this.isProjectExiting = false;
            this.sendMessage();
            this.cdr.markForCheck();
        }, 160);
    }


    getProjectConversation(projectId?: string) {
        if (!projectId) {
            return;
        }
        this.projectService.getProjectConversation(projectId).subscribe({
            next: (res) => {
                const convs = res as ProjectConversationInfo[];
                this.projectConversation = convs;
                this.projectConversationMap = { ...this.projectConversationMap, [projectId]: convs };
                this.cdr.markForCheck();
            }, error: (err) => {
                console.log(err);
                this.snack.showErrorMessage(err.error);
            }
        });
    }

    get pinnedConversations(): Conversation[] {
        return this.conversations.filter(c => c.isPinned);
    }
}
