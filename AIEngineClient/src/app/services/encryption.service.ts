import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { EngineCore } from './engine-core';
import { EngineControllers, ExcludeEncryptionEndPoints } from './engine-route-constants';

@Injectable({
    providedIn: 'root'
})

export class EncryptionService {
    private http = inject(EngineCore);
    private publicKey: CryptoKey | null = null;
    private readonly initializePromise: Promise<void>;

    constructor() {
        this.initializePromise = this.initialize();
    }

    private async initialize(): Promise<void> {
        const pem = await firstValueFrom(
            await this.http.getText(`${EngineControllers.EncryptionController}/public-key`)
        );

        this.publicKey = await this.importPublicKey(pem);
    }

    async encrypt(data: unknown): Promise<string> {
        await this.initializePromise;

        if (!this.publicKey) {
            throw new Error('Public key not loaded.');
        }

        const json = JSON.stringify(data);
        const encoded = new TextEncoder().encode(json);
        const encrypted = await crypto.subtle.encrypt(
            {
                name: 'RSA-OAEP'
            },
            this.publicKey,
            encoded
        );

        return this.arrayBufferToBase64(encrypted);
    }

    private async importPublicKey(pem: string): Promise<CryptoKey> {
        const pemContents = pem
            .replace('-----BEGIN PUBLIC KEY-----', '')
            .replace('-----END PUBLIC KEY-----', '')
            .replace(/\s/g, '');

        const binary = atob(pemContents);
        const bytes = new Uint8Array(binary.length);

        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }

        return crypto.subtle.importKey(
            'spki',
            bytes.buffer,
            {
                name: 'RSA-OAEP',
                hash: 'SHA-256'
            },
            false,
            ['encrypt']
        );
    }

    private arrayBufferToBase64(buffer: ArrayBuffer): string {
        const bytes = new Uint8Array(buffer);
        let binary = '';
        bytes.forEach(b => binary += String.fromCharCode(b));

        return btoa(binary);
    }
}