import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface LoaderConfig {
    message?: string;
}

@Injectable({
    providedIn: 'root'
})
export class LoaderService {

    /** Emits a config to show the loader, or null to hide it. */
    loader$ = new BehaviorSubject<LoaderConfig | null>(null);

    /** Show the global loader with an optional message. */
    show(message?: string): void {
        this.loader$.next({ message });
    }

    /** Hide the global loader. */
    hide(): void {
        this.loader$.next(null);
    }
}
