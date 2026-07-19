import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { LoaderService, LoaderConfig } from '../../services/loader-service';
import { BrandLogoSvg } from '../svgs/brand-logo-svg/brand-logo-svg';

@Component({
	selector: 'app-loader',
	standalone: true,
	imports: [CommonModule, BrandLogoSvg],
	templateUrl: './loader.html',
	styleUrl: './loader.css'
})
export class Loader implements OnInit, OnDestroy {
	config = signal<LoaderConfig | null>(null);
	visible = signal(false);

	private sub!: Subscription;
	private hideTimer?: any;

	constructor(private loaderService: LoaderService) { }

	ngOnInit(): void {
		this.sub = this.loaderService.loader$.subscribe(cfg => {
			if (cfg !== null) {
				clearTimeout(this.hideTimer);
				this.config.set(cfg);
				this.visible.set(true);
			} else {
				// let the CSS fade-out play before unmounting
				this.hideTimer = setTimeout(() => {
					this.config.set(null);
					this.visible.set(false);
				}, 400);
			}
		});
	}

	ngOnDestroy(): void {
		this.sub?.unsubscribe();
		clearTimeout(this.hideTimer);
	}
}
