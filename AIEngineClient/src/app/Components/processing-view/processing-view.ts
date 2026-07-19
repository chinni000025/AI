import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../services/theme.service';
import { EngineService } from '../../services/engine-service';
import { Router } from '@angular/router';
import { SnackbarService } from '../../services/snackbar-service';
import { interval, Subject, switchMap, takeUntil } from 'rxjs';
import { SignalRService } from '../../services/signalr-service';
import { EngineConstants, HubEndpoints } from '../../services/engine-route-constants';

@Component({
    selector: 'app-processing-view',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './processing-view.html',
    styleUrl: './processing-view.css'
})
export class ProcessingView implements OnInit, OnDestroy {
    title: string = 'System Synchronizing';
    message: string = 'We are working on bringing things up to speed. This may take a moment while we apply critical updates and migrations.';
    private destroy$ = new Subject<void>();
    public get titleChars(): string[] {
        return this.title.split('');
    }

    constructor(public themeService: ThemeService,
        private engineService: EngineService,
        private router: Router,
        private snack: SnackbarService,
        private signalr: SignalRService
    ) { }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.signalr.unsubscribeHub(HubEndpoints.EngineStatusHub, EngineConstants.EngineStateChanged);
    }

    ngOnInit(): void {
        this.isAlreadyEngineStarted();
        this.signalr.subscribeHub<any>(HubEndpoints.EngineStatusHub, EngineConstants.EngineStateChanged)
            .pipe(takeUntil(this.destroy$)).subscribe(status => {
                if (status?.isEngineReady) {
                    this.router.navigate([''], { replaceUrl: true });
                    return;
                }
                if (status?.errorMessage) {
                    this.snack.showErrorMessage(status.errorMessage);
                }
            });
    }

    private isAlreadyEngineStarted() {
        this.engineService.getEngineState().subscribe({
            next: (status) => {

                if (status.isEngineReady) {
                    this.router.navigate([''], { replaceUrl: true });
                    return;
                }
                if (status.errorMessage) {
                    this.snack.showErrorMessage(status.errorMessage);
                }
            },
            error: () => {
                this.message = "Waiting for the AI Engine service to respond...";
            }
        });
    }
}