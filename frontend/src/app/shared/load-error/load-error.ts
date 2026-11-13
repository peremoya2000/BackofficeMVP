import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { toUserMessage } from '../../core/http/http-error.interceptor';

const UNKNOWN_ERROR_MESSAGE = 'No se han podido cargar los datos.';

@Component({
  imports: [MatButtonModule, MatIconModule],
  selector: 'app-load-error',
  host: { role: 'alert' },
  template: `
    <mat-icon>error_outline</mat-icon>
    <p>{{ message() }}</p>
    <button matButton="outlined" type="button" (click)="retry.emit()">
      <mat-icon>refresh</mat-icon>
      Reintentar
    </button>
  `,
  styles: `
    :host {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
      padding: 32px 16px;
      text-align: center;
      color: var(--mat-sys-on-surface-variant);
    }

    p {
      margin: 0;
    }
  `,
})
export class LoadError {
  readonly error = input.required<unknown>();
  readonly retry = output();

  protected readonly message = computed(() => {
    const error = this.error();
    return error instanceof HttpErrorResponse ? toUserMessage(error) : UNKNOWN_ERROR_MESSAGE;
  });
}
