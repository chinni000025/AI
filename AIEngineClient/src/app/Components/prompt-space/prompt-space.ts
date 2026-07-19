import { Component, ElementRef, HostListener, ViewChild, AfterViewChecked, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmationDialog } from '../dialogs/confirmation-dialog/confirmation-dialog';
import { Buttons, CatalogModel, CatalogProvider, EngineConstants, EngineRoutes, HubConfiguration, HubEndpoints, VoiceStatusLabel } from '../../services/engine-route-constants';
import { DialogService } from '../../services/dialog.service';
import { ActivatedRoute, Router } from '@angular/router';
import { SignalRService } from '../../services/signalr-service';
import { ChatMessage, Conversation } from '../../models/snackbar-config';
import { ThemeService } from '../../services/theme.service';
import { LoaderService } from '../../services/loader-service';
import { SnackbarService } from '../../services/snackbar-service';
import { IdentityService } from '../../services/identity-service';
import { finalize, timestamp } from 'rxjs';
import { PinSvg } from "../svgs/pin-svg/pin-svg";
import { RenameSvg } from "../svgs/rename-svg/rename-svg";
import { ArchiveSvg } from "../svgs/archive-svg/archive-svg";
import { ShareSvg } from "../svgs/share-svg/share-svg";
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
import { ChatService } from '../../services/chat-service';
import { ArchiveDialog } from '../dialogs/archive-dialog/archive-dialog';
import { SettingsDialog } from '../dialogs/settings-dialog/settings-dialog';
import { MarkdownModule } from 'ngx-markdown';
import { PromptSpaceService } from '../../services/prompt-space.service';
import { TokenService } from '../../services/token-service';

@Component({
    selector: 'app-prompt-space',
    imports: [FormsModule, PinSvg, RenameSvg,
        ArchiveSvg, ShareSvg, TrashSvg, FavoriteSvg, ThunderSvg,
        ChatsSvg, BrandLogoSvg, UserSvg,
        MicSvg, AttachmentSvg, SendSvg, ExpandCollapseSvg,
        PlusSvg, SettingsSvg, LogoutSvg, SunSvg, MoonSvg, KebabMenuSvg, DropdownSvg, ArchiveDialog, SettingsDialog, MarkdownModule],
    templateUrl: './prompt-space.html',
    styleUrl: './prompt-space.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PromptSpace implements AfterViewChecked {
    sidebarCollapsed = false;

    /** Active tab in the sidebar: 'chats' | 'saved' */
    activeTab: 'chats' | 'saved' = 'chats';
    activeConversationId: string | null = null;
    editingConversationId: string | null = null;
    openMenuId: string | null = null;
    menuOpensUp = false;
    favourites = new Set<string>();
    isTyping = false;
    promptText = '';

    isVoiceModalOpen = false;
    voiceState: 'idle' | 'listening' | 'processing' | 'done' = 'idle';
    voiceTranscript = '';
    voiceBars = [0.4, 0.7, 0.9, 0.6, 1.0, 0.5, 0.8, 0.3, 0.65, 0.85, 0.45, 0.75];
    get voiceStatusLabel(): string {
        switch (this.voiceState) {
            case VoiceStatusLabel.Listening: return 'Listening…';
            case VoiceStatusLabel.Processing: return 'Processing speech…';
            case VoiceStatusLabel.Done: return 'Transcription complete';
            default: return 'Tap the mic to speak';
        }
    }
    private mediaRecorder!: MediaRecorder;
    private audioChunks: Blob[] = [];
    private recordedBlod!: Blob;
    private audioContext!: AudioContext;
    private analyser!: AnalyserNode; // for voice bars.
    private dataArray!: Uint8Array<ArrayBuffer>; //frequency data. 0-->255.
    private animatedFrameId!: number;

    // ── Text-to-Speech UI state ─────────────────────────────────
    /** ID of the message currently being "spoken". null = none. */
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

    constructor(
        private dialogService: DialogService,
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
        private tokenService: TokenService
    ) { }

    ngOnInit() {
        this.sidebarCollapsed = window.innerWidth < 768;
        this.getModels();
        this.userName = this.getUserName();
        this.loadConversations();

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
            if (conversationId) {
                this.loadConversation(conversationId);
            }
        });
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
        clearTimeout(this.enableTitleEditTimeout);
        clearTimeout(this.speakStopTimeout);
        clearTimeout(this.copiedMessageResetTimeout);

        if (this.scrollTimeout) {
            clearTimeout(this.scrollTimeout);
            this.scrollTimeout = null;
        }

        this.signalr.unsubscribeHub(HubEndpoints.NotificationHub, EngineConstants.ForceLogout);
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
        this.loadConversations();
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
            subMessage: `"${conv.conversationTitle}" will be moved to the AIEngine Recycle Bin and permanently deleted after 24 hours.\n You can restore it anytime before deletion.`,
            iconType: 'trash-svg',
            variant: 'danger',
            buttons: [Buttons.Cancel, Buttons.Delete]
        }).afterClosed().subscribe((result: any) => {
            if (result) {
                this.chatService.deleteConversation(conv.conversationId).subscribe({
                    next: (res) => {
                        this.conversations = this.conversations.filter(c => c.conversationId !== conv.conversationId);
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
        this.messages = [];
        this.promptText = '';
        this.shouldScrollToBottom = true;
    }

    loadConversation(conversationId: string): void {
        this.activeConversationId = conversationId;
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
        this.isVoiceModalOpen = true;
        this.voiceState = 'idle';
        this.voiceTranscript = '';
        this.cdr.markForCheck();
    }

    closeVoiceModal(): void {
        this.isVoiceModalOpen = false;
        this.voiceState = 'idle';
        this.voiceTranscript = '';
        this.cdr.markForCheck();
    }

    toggleVoiceRecording(): void {
        if (this.voiceState === 'idle' || this.voiceState === 'done') {
            //Ask media to broswer and returns stream from microphone.
            navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
                this.mediaRecorder = new MediaRecorder(stream);
                this.audioChunks = [];
                this.mediaRecorder.ondataavailable = (event) => { // audio chunks.
                    this.audioChunks.push(event.data);
                }
                this.mediaRecorder.start();
                //audio processing engine.
                this.audioContext = new AudioContext();
                const source = this.audioContext.createMediaStreamSource(stream);
                this.analyser = this.audioContext.createAnalyser(); //freq analyzer
                //fast fourier transform time domain --> freq domain.
                this.analyser.fftSize = 64;
                const bufferlength = this.analyser.frequencyBinCount; //fft/2.
                this.dataArray = new Uint8Array(bufferlength); //8 bit --> 256 bytes.
                source.connect(this.analyser);
                this.startVisualizer();
                this.voiceState = 'listening';
                this.cdr.detectChanges();
            }).catch(err => {
                console.log("err");
            });
        } else if (this.voiceState === 'listening') {
            this.voiceState = 'processing';
            this.mediaRecorder.stop();
            this.mediaRecorder.onstop = () => {
                this.recordedBlod = new Blob(this.audioChunks, { type: 'audio/webm' });
                cancelAnimationFrame(this.animatedFrameId);
                if (this.audioContext) {
                    this.audioContext.close();
                }
                this.sendAudioToBackend(this.recordedBlod);
                this.mediaRecorder.stream.getTracks().forEach(track => track.stop()); //stoping microphone.
            }
        }
        this.cdr.markForCheck();
    }

    startVisualizer() {
        const update = () => {
            this.analyser.getByteFrequencyData(this.dataArray);
            this.voiceBars = Array.from(this.dataArray)
                .slice(0, 12)
                .map(v => v / 255);

            this.cdr.markForCheck();
            this.animatedFrameId = requestAnimationFrame(update); //60fps.
        };
        update();
    }

    sendAudioToBackend(audioBlob: Blob) {
        const formData = new FormData();
        formData.append('audioFile', audioBlob, 'recording.webm');
        this.chatService.uploadAudio(formData).subscribe({
            next: (res: any) => {
                if (!res?.text || res?.text == '[BLANK_AUDIO]') {
                    this.voiceTranscript = '';
                    this.voiceState = 'idle';
                    this.cdr.markForCheck();
                    return;
                }
                this.voiceTranscript = res.text;
                this.voiceState = 'done';
                this.cdr.markForCheck();
            },
            error: (err: any) => {
                console.error(err);
                this.voiceState = 'idle';
            }
        });
    }


    useVoiceTranscript(): void {
        if (this.voiceTranscript.trim()) {
            this.promptText = this.voiceTranscript.trim();
        }
        this.closeVoiceModal();
        this.cdr.markForCheck();
    }

    onVoiceInput(): void {
        this.openVoiceModal();
    }

    onAttach(): void {
        console.log('[PromptSpace] Attach file requested');
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


    /*   Futher Case.
    // ── Text-to-Speech UI methods (UI-only – backend will wire real TTS) ──────
    toggleSpeak(msg: ChatMessage, event: MouseEvent): void {
        event.stopPropagation();
    
        // If already speaking this message, stop it
        if (this.speakingMsgId === msg.id) {
            this.stopSpeak();
            return;
        }
    
        // Cancel any in-progress speech first
        clearTimeout(this.speakStopTimeout);
    
        // Set new speaking ID and NEW array reference so @for re-evaluates
        // every item's bindings (required for OnPush + track-by-id loops)
        this.speakingMsgId = msg.id;
        this.messages = [...this.messages];
        this.cdr.detectChanges();
    
        // Auto-stop after estimated read duration
        const wordCount = (msg.content?.toString() ?? '').split(/\s+/).filter(Boolean).length;
        const durationMs = Math.max(2500, Math.min(wordCount * 450, 30000));
        this.speakStopTimeout = setTimeout(() => {
            this.stopSpeak();
        }, durationMs);
    }
    
    stopSpeak(): void {
        clearTimeout(this.speakStopTimeout);
        if (this.speakingMsgId === null) return; // already stopped — no-op
        this.speakingMsgId = null;
        this.messages = [...this.messages]; // force @for re-evaluation
        this.cdr.detectChanges();
    }
    */

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

    sendMessage(): void {
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
        const payload = {
            Prompt: text,
            Model: this.selectedModel,
            Provider: this.selectedProvider
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
                console.error('Error fetching AI response:', err);
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
}
