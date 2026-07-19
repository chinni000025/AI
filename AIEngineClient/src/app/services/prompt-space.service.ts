import { inject, Injectable } from '@angular/core';
import { EngineCore } from './engine-core';
import { ArchiveChat, EngineControllers, PaginationResponse } from './engine-route-constants';
@Injectable({
    providedIn: 'root',
})
export class PromptSpaceService {
    private http = inject(EngineCore);
    constructor() { }
    getModels(): any {
        return this.http.get(`${EngineControllers.DashboardController}/GetModels`);
    }

    getArchivedChat(page: number, pageSize: number, search: string): any {
        let url = `${EngineControllers.ConversationController}/GetArchiveChats?page=${page}&pageSize=${pageSize}`;
        if (search) {
            url += `&search=${search}`;
        }
        return this.http.get<PaginationResponse<ArchiveChat>>(url);
    }
}