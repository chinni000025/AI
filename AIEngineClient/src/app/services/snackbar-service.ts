import { SnackbarConfig } from "../models/snackbar-config";
import { Subject } from "rxjs";
import { Injectable } from "@angular/core";
import { EngineConstants } from "./engine-route-constants";

@Injectable({
	providedIn: 'root'
})
export class SnackbarService {
	private snackbarQueue: SnackbarConfig[] = [];
	private isDisplaying = false;
	private timerId?: any;

	snackbar$ = new Subject<SnackbarConfig | null>();

	showSuccessMessage(message: string, duration: number = 3000, position: any = EngineConstants.Top) {
		this.show({
			message: message,
			type: EngineConstants.Success,
			duration: duration,
			position: position
		});
	}

	showInfoMessage(message: string, duration: number = 3000, position: any = EngineConstants.Top) {
		this.show({
			message: message,
			type: EngineConstants.Info,
			duration: duration,
			position: position
		});
	}

	showWarningMessage(message: string, duration: number = 3000, position: any = EngineConstants.Top) {
		this.show({
			message: message,
			type: EngineConstants.Warning,
			duration: duration,
			position: position
		});
	}

	showErrorMessage(message: string, duration: number = 3000, position: any = EngineConstants.Top) {
		this.show({
			message: message,
			type: EngineConstants.Error,
			duration: duration,
			position: position
		});
	}

	show(config: SnackbarConfig) {
		this.snackbarQueue.push(config);
		this.processQueue();
	}

	dismiss() {
		if (this.timerId && this.isDisplaying) {
			clearTimeout(this.timerId);
			this.timerId = undefined; //timer cleared immediately on dismiss
			this.hideAndProcessNext();
		}
	}

	private hideAndProcessNext() {
		this.snackbar$.next(null);

		// Wait for UI transition before processing next item
		setTimeout(() => {
			this.isDisplaying = false;
			this.processQueue();
		}, 300);
	}

	private processQueue() {
		if (this.isDisplaying || this.snackbarQueue.length === 0) return;

		this.isDisplaying = true;

		const config = this.snackbarQueue.shift()!;
		this.snackbar$.next(config);

		const duration = config.duration || 3000;

		this.timerId = setTimeout(() => {
			this.timerId = undefined; //timer should be cleared after the duration
			this.hideAndProcessNext();
		}, duration);
	}
}