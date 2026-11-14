import { Component, computed, input } from '@angular/core';
import { BOOKING_STATUS_LABELS, BookingStatus } from '../data/booking-status';

@Component({
  selector: 'app-booking-status-chip',
  template: '{{ label() }}',
  styles: `
    :host {
      padding: 2px 10px;
      border-radius: var(--mat-sys-corner-full);
      font: var(--mat-sys-label-large);
      white-space: nowrap;
    }

    :host(.confirmed) {
      background-color: #d4eed9;
      color: #1d5b2a;
    }

    :host(.cancelled) {
      background-color: var(--mat-sys-surface-container-highest);
      color: var(--mat-sys-on-surface-variant);
    }
  `,
  host: {
    '[class.confirmed]': "status() === 'Confirmed'",
    '[class.cancelled]': "status() === 'Cancelled'",
  },
})
export class BookingStatusChip {
  readonly status = input.required<BookingStatus>();

  protected readonly label = computed(() => BOOKING_STATUS_LABELS[this.status()]);
}
