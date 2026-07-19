import { Component, Inject, InjectionToken, Optional } from '@angular/core';
import { InfoSvg } from '../../svgs/info-svg/info-svg';
import { WarningSvg } from '../../svgs/warning-svg/warning-svg';
import { ErrorSvg } from '../../svgs/error-svg/error-svg';
import { SuccessSvg } from '../../svgs/success-svg/success-svg';
import { TrashSvg } from '../../svgs/trash-svg/trash-svg';
import { ShieldSvg } from '../../svgs/shield-svg/shield-svg';

export interface DialogButton {
	text: string;
	value?: any;
	variant?: 'primary' | 'secondary' | 'danger' | 'warning' | 'info' | 'ghost'; // Button styling
	action?: () => void;
}

export interface ConfirmationDialogData {
	width?: string;
	height?: string;
	message: string;
	subMessage?: string;
	iconType?: string;
	variant?: 'info' | 'warning' | 'confirmation' | 'danger' | 'default';
	buttons?: DialogButton[];
}

export const DIALOG_DATA = new InjectionToken<ConfirmationDialogData>('DIALOG_DATA');
export const DIALOG_REF = new InjectionToken<any>('DIALOG_REF');

@Component({
	selector: 'app-confirmation-dialog',
	imports: [InfoSvg, WarningSvg, ErrorSvg, SuccessSvg, TrashSvg, ShieldSvg],
	templateUrl: './confirmation-dialog.html',
	styleUrl: './confirmation-dialog.css',
})
export class ConfirmationDialog {
	data: ConfirmationDialogData;

	constructor(
		@Optional() @Inject(DIALOG_DATA) data: ConfirmationDialogData,
		@Optional() @Inject(DIALOG_REF) private dialogRef: any
	) {
		this.data = data || { message: 'Are you sure?' };
		this.data.variant = this.data.variant || 'default';
		this.data.iconType = this.data.iconType || 'info-svg';

		if (!this.data.buttons || this.data.buttons.length === 0) {
			this.data.buttons = [
				{ text: 'OK', value: true, variant: 'primary' }
			];
		}
	}

	onButtonClick(btn: DialogButton): void {
		if (btn.action) {
			btn.action();
		}
		if (this.dialogRef && this.dialogRef.close) {
			this.dialogRef.close(btn.value !== undefined ? btn.value : true);
		}
	}
}