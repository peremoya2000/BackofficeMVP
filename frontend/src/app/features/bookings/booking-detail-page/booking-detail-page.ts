import { CurrencyPipe, DatePipe, I18nPluralPipe } from '@angular/common';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RouterLink } from '@angular/router';
import { differenceInCalendarDays, isBefore, parseISO, startOfToday } from 'date-fns';
import { filter, switchMap, tap } from 'rxjs';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/confirm-dialog/confirm-dialog';
import { PageHeader } from '../../../shared/page-header/page-header';
import { BookingStatusChip } from '../booking-status-chip/booking-status-chip';
import { BOOKING_CHANNEL_LABELS } from '../data/booking-channel';
import { BookingsApi } from '../data/bookings-api';

const CANCELLED_SNACK_BAR_DURATION_MS = 4000;

@Component({
  imports: [
    CurrencyPipe,
    DatePipe,
    I18nPluralPipe,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    PageHeader,
    BookingStatusChip,
  ],
  selector: 'app-booking-detail-page',
  styleUrl: './booking-detail-page.scss',
  templateUrl: './booking-detail-page.html',
})
export class BookingDetailPage {
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly bookingsApi = inject(BookingsApi);

  readonly id = input.required<string>();

  protected readonly channelLabels = BOOKING_CHANNEL_LABELS;
  protected readonly nightsPlural = { '=1': '# noche', other: '# noches' };
  protected readonly adultsPlural = { '=1': '# adulto', other: '# adultos' };
  protected readonly childrenPlural = { '=1': '# niño', other: '# niños' };
  protected readonly cancelling = signal(false);

  protected readonly booking = rxResource({
    params: this.id,
    stream: ({ params: id }) => this.bookingsApi.getById(id),
  });

  protected readonly nights = computed(() => {
    if (!this.booking.hasValue()) return 0;
    const { checkIn, checkOut } = this.booking.value();
    return differenceInCalendarDays(parseISO(checkOut), parseISO(checkIn));
  });

  // The server's rule: only confirmed bookings, and only before check-in.
  protected readonly canCancel = computed(() => {
    if (!this.booking.hasValue()) return false;
    const { status, checkIn } = this.booking.value();
    return status === 'Confirmed' && !isBefore(parseISO(checkIn), startOfToday());
  });

  protected cancel(): void {
    if (!this.booking.hasValue()) return;
    const { id, reference } = this.booking.value();

    const data: ConfirmDialogData = {
      title: `¿Cancelar la reserva ${reference}?`,
      message:
        'Sus habitaciones volverán a estar disponibles en todas sus noches. No se puede deshacer.',
      confirmLabel: 'Cancelar reserva',
    };
    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed()
      .pipe(
        filter((confirmed) => confirmed === true),
        tap(() => this.cancelling.set(true)),
        switchMap(() => this.bookingsApi.cancel(id)),
      )
      .subscribe({
        next: () => {
          this.cancelling.set(false);
          this.snackBar.open('Reserva cancelada.', 'Cerrar', {
            duration: CANCELLED_SNACK_BAR_DURATION_MS,
          });
          this.booking.reload();
        },
        error: (error: HttpErrorResponse) => {
          this.cancelling.set(false);
          if (error.status === HttpStatusCode.Conflict) this.booking.reload();
        },
      });
  }
}
