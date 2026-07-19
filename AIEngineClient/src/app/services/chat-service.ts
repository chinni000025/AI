import { inject, Injectable } from '@angular/core';
import { EngineCore } from './engine-core';
import { ArchiveChat, EngineControllers, PaginationResponse } from './engine-route-constants';
@Injectable({
    providedIn: 'root',
})
export class ChatService {
    private http = inject(EngineCore);

    constructor() { }

    sendNewMessage(payload: any) {
        return this.http.post(`${EngineControllers.ConversationController}/send/message`, payload);
    }

    sendMessage(conversationId: string, payload: any) {
        return this.http.post(`${EngineControllers.ConversationController}/send/${conversationId}/message`, payload);
    }

    getAllConversations() {
        return this.http.get(`${EngineControllers.ConversationController}/GetConversations`);
    }

    getFavouriteConversations() {
        return this.http.get(`${EngineControllers.ConversationController}/GetFavouriteConversations`);
    }

    getConversationsById(conversationId: any) {
        return this.http.get(`${EngineControllers.ConversationController}/GetConversation/${conversationId}`);
    }

    updateConversation(conversationId: any, operation: any) {
        return this.http.patch(`${EngineControllers.ConversationController}/Update/${conversationId}`, operation);
    }

    deleteConversation(conversationId: any) {
        return this.http.delete(`${EngineControllers.ConversationController}/Delete/${conversationId}`, {});
    }

    uploadAudio(formData: FormData) {
        return this.http.post(`${EngineControllers.ConversationController}/speech/transcribe`, formData);
    }

    getModels(): any {
        return this.http.get(`${EngineControllers.DashboardController}/GetModels`);
    }

}
