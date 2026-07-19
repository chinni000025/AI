import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { SnackbarService } from '../../services/snackbar-service';
import { SnackbarConfig } from '../../models/snackbar-config';
import { Subscription } from 'rxjs';
import { SuccessSvg } from '../svgs/success-svg/success-svg';
import { ErrorSvg } from '../svgs/error-svg/error-svg';
import { WarningSvg } from '../svgs/warning-svg/warning-svg';
import { InfoSvg } from '../svgs/info-svg/info-svg';
import { CloseSvg } from '../svgs/close-svg/close-svg';

@Component({
  selector: 'app-snackbar',
  imports: [CommonModule, SuccessSvg, ErrorSvg, WarningSvg, InfoSvg, CloseSvg],
  templateUrl: './snackbar.html',
  styleUrl: './snackbar.css',
})
export class Snackbar implements OnInit, OnDestroy {
  currentSnackbar: SnackbarConfig | null = null;
  visible = false;
  private subscription?: Subscription;

  constructor(
    private snackbarService: SnackbarService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.subscription = this.snackbarService.snackbar$.subscribe(config => {
      if (config) {
        this.currentSnackbar = config;
        this.visible = true;
      } else {
        this.visible = false;
      }
      this.cdr.detectChanges();
    });
  }

  close() {
    this.snackbarService.dismiss();
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
