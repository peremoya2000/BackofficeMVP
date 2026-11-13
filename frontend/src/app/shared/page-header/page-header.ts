import { Location } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';

@Component({
  imports: [MatButtonModule, MatIconModule],
  selector: 'app-page-header',
  template: `
    @if (backLink(); as link) {
      <button matIconButton type="button" aria-label="Volver" (click)="goBack(link)">
        <mat-icon>arrow_back</mat-icon>
      </button>
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
  private readonly router = inject(Router);
  private readonly location = inject(Location);

  readonly title = input.required<string>();
  readonly backLink = input<string>();

  // History keeps the previous page's URL state (search, page, week); a page opened directly has none.
  protected goBack(fallbackLink: string): void {
    if (this.router.lastSuccessfulNavigation()?.previousNavigation) {
      this.location.back();
    } else {
      this.router.navigateByUrl(fallbackLink);
    }
  }
}
