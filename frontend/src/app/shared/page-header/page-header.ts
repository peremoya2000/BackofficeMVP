import { Component, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink, MatButtonModule, MatIconModule],
  selector: 'app-page-header',
  template: `
    @if (backLink(); as link) {
      <a matIconButton [routerLink]="link" aria-label="Volver">
        <mat-icon>arrow_back</mat-icon>
      </a>
    }
    <h1>{{ title() }}</h1>
    <ng-content />
  `,
  styles: `
    :host {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 16px;
    }

    h1 {
      flex: 1;
      margin: 0;
      font: var(--mat-sys-headline-small);
    }
  `,
})
export class PageHeader {
  readonly title = input.required<string>();
  readonly backLink = input<string>();
}
