import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.developement';
@Injectable({
    providedIn: 'root',
})
export class EngineCore {
    private baseUrl = environment.apiUrl;
    constructor(private httpClient: HttpClient) { }

    post<T>(endpoint: any, payload: any): Observable<T> {
        return this.httpClient.post<T>(`${this.baseUrl}/${endpoint}`, payload, { withCredentials: true })
    }

    get<T>(endpoint: any): Observable<T> {
        return this.httpClient.get<T>(`${this.baseUrl}/${endpoint}`, { withCredentials: true });
    }

    delete<T>(endpoint: any, params: any) {
        return this.httpClient.delete<T>(`${this.baseUrl}/${endpoint}`, { params, withCredentials: true });
    }

    patch<T>(endpoint: any, operation: any) {
        return this.httpClient.patch<T>(`${this.baseUrl}/${endpoint}`, operation,
            {
                headers: { 'Content-Type': 'application/json-patch+json' },
                withCredentials: true
            }
        );
    }

    getText(endpoint: string): Observable<string> {
        return this.httpClient.get(`${this.baseUrl}/${endpoint}`,
            {
                responseType: 'text',
            }
        );
    }
}
