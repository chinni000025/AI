import { Injectable, ApplicationRef, Injector, EnvironmentInjector, createComponent, Type } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { DIALOG_DATA, DIALOG_REF } from '../Components/dialogs/confirmation-dialog/confirmation-dialog';

export class DialogRef {
	private _afterClosed = new Subject<any>();
	private componentRef: any;

	constructor(private appRef: ApplicationRef) { }

	setComponentRef(ref: any) {
		this.componentRef = ref;
	}

	close(result?: any) {
		this._afterClosed.next(result);
		this._afterClosed.complete();

		if (this.componentRef) {
			this.appRef.detachView(this.componentRef.hostView);
			this.componentRef.destroy();
		}
	}

	afterClosed(): Observable<any> {
		return this._afterClosed.asObservable();
	}
}

@Injectable({
	providedIn: 'root'
})
export class DialogService {
	constructor(
		private appRef: ApplicationRef,
		private injector: Injector,
		private environmentInjector: EnvironmentInjector
	) { }

	/**
	 * Opens the requested dialog component dynamically, passing data to it via DIALOG_DATA.
	 * Returns a DialogRef that exposes an afterClosed() Observable.
	 */
	open<T>(componentType: Type<T>, data: any): DialogRef {
		// 1. Setup the Remote Control logic specifically handling teardown internally
		const dialogRef = new DialogRef(this.appRef);

		// 2. Hydrate Injectors with necessary bindings
		const customInjector = Injector.create({
			providers: [
				{ provide: DIALOG_DATA, useValue: data },
				{ provide: DIALOG_REF, useValue: dialogRef }
			],
			parent: this.injector
		});

		// 3. Mount component instance
		const componentRef = createComponent(componentType, {
			environmentInjector: this.environmentInjector,
			elementInjector: customInjector
		});

		dialogRef.setComponentRef(componentRef);

		// 4. Attach to Angular rendering application & push into Document Flow
		document.body.appendChild(componentRef.location.nativeElement);
		this.appRef.attachView(componentRef.hostView);

		return dialogRef;
	}
}
