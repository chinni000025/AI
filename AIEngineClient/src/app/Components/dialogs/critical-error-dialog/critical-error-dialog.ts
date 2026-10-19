import { Component, Inject, InjectionToken, Optional, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ErrorSvg } from '../../svgs/error-svg/error-svg';
import { CloseSvg } from '../../svgs/close-svg/close-svg';

export interface CriticalErrorDialogData {
  title?: string;
  message: string;
  correlationId?: string;
  statusCode?: number;
  errorCode?: string;
  okText?: string;
  cancelText?: string;
}

export const CRITICAL_ERROR_DIALOG_DATA = new InjectionToken<CriticalErrorDialogData>('CRITICAL_ERROR_DIALOG_DATA');
export const CRITICAL_ERROR_DIALOG_REF = new InjectionToken<any>('CRITICAL_ERROR_DIALOG_REF');

@Component({
  selector: 'app-critical-error-dialog',
  imports: [CommonModule, ErrorSvg, CloseSvg],
  templateUrl: './critical-error-dialog.html',
  styleUrl: './critical-error-dialog.css'
})
export class CriticalErrorDialog {
  data: CriticalErrorDialogData;
  copied = false;

  constructor(
    @Optional() @Inject(CRITICAL_ERROR_DIALOG_DATA) data: CriticalErrorDialogData,
    @Optional() @Inject(CRITICAL_ERROR_DIALOG_REF) private dialogRef: any,
    private cdr: ChangeDetectorRef
  ) {
    this.data = data || {
      title: 'System Exception',
      message: 'An unexpected error occurred while processing your request.',
      okText: 'OK',
      cancelText: 'Cancel'
    };
  }

  copyCorrelationId(event: MouseEvent): void {
    // Left for you to customize/finalize
    event.stopPropagation();
    if (!this.data?.correlationId) return;

    navigator.clipboard.writeText(this.data.correlationId).then(() => {
      this.copied = true;
      this.cdr.detectChanges();
      setTimeout(() => {
        this.copied = false;
        this.cdr.detectChanges();
      }, 2500);
    });
  }

  onOk(): void {
    // Left for you to customize/finalize
    if (this.dialogRef?.close) {
      this.dialogRef.close('ok');
    }
  }

  onCancel(): void {
    // Left for you to customize/finalize
    if (this.dialogRef?.close) {
      this.dialogRef.close('cancel');
    }
  }
}

