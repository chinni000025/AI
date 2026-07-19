import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { DOCUMENT, isPlatformBrowser } from '@angular/common';

@Injectable({
	providedIn: 'root'
})
export class ThemeService {
	private currentTheme: 'light' | 'dark' = 'dark';

	constructor(
		@Inject(DOCUMENT) private document: Document,
		@Inject(PLATFORM_ID) private platformId: Object
	) {
		this.initTheme();
	}

	private initTheme(): void {
		if (isPlatformBrowser(this.platformId)) {
			const savedTheme = localStorage.getItem('theme') as 'light' | 'dark';

			if (savedTheme) {
				this.setTheme(savedTheme);
			} else {
				// Check OS preference
				const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
				this.setTheme(prefersDark ? 'dark' : 'light');
			}
		}
	}

	toggleTheme(): void {
		const newTheme = this.currentTheme === 'dark' ? 'light' : 'dark';
		this.setTheme(newTheme);
	}

	setTheme(theme: 'light' | 'dark'): void {
		this.document.body.classList.remove(this.currentTheme);
		this.currentTheme = theme;
		this.document.body.classList.add(this.currentTheme);

		if (isPlatformBrowser(this.platformId)) {
			localStorage.setItem('theme', theme);
		}
	}

	getTheme(): 'light' | 'dark' {
		return this.currentTheme;
	}

	isDarkMode(): boolean {
		return this.currentTheme === 'dark';
	}
}