import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { differenceInCalendarDays, parseISO, startOfToday } from 'date-fns';
import { debounceTime, map } from 'rxjs';
import { toUserMessage } from '../../../core/http/http-error.interceptor';
import { toIsoDate } from '../../../shared/dates/iso-date';
import { BOOKING_CHANNEL_LABELS, BookingChannel } from '../data/booking-channel';
import { BookingResponse } from '../data/booking-response';
import { BookingsApi } from '../data/bookings-api';

const QUOTE_DEBOUNCE_MS = 300;

export interface BookingCreateDialogData {
  hotelName: string;
  roomTypes: { id: string; name: string }[];
}

interface QuoteParams {
  roomTypeId: string;
  checkIn: string;
  checkOut: string;
  rooms: number;
}

@Component({
  imports: [
    CurrencyPipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatDatepickerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  selector: 'app-booking-create-dialog',
  styleUrl: './booking-create-dialog.scss',
  templateUrl: './booking-create-dialog.html',
})
export class BookingCreateDialog {
  private readonly dialogRef =
    inject<MatDialogRef<BookingCreateDialog, BookingResponse>>(MatDialogRef);
  private readonly bookingsApi = inject(BookingsApi);

  protected readonly data = inject<BookingCreateDialogData>(MAT_DIALOG_DATA);
  protected readonly today = startOfToday();
  protected readonly channels = Object.entries(BOOKING_CHANNEL_LABELS) as [
    BookingChannel,
    string,
  ][];
  protected readonly saving = signal(false);

  protected readonly form = new FormGroup({
    // Only the stay changes the price, so only these fields ask for a new quote.
    stay: new FormGroup({
      roomTypeId: new FormControl(this.data.roomTypes[0].id, {
        nonNullable: true,
        validators: Validators.required,
      }),
      checkIn: new FormControl<Date | null>(null, Validators.required),
      checkOut: new FormControl<Date | null>(null, Validators.required),
      rooms: new FormControl(1, {
        nonNullable: true,
        validators: [Validators.required, Validators.min(1)],
      }),
    }),
    adults: new FormControl(2, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(1)],
    }),
    children: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0)],
    }),
    leadGuestName: new FormControl('', { nonNullable: true, validators: Validators.required }),
    channel: new FormControl<BookingChannel>('Agency', { nonNullable: true }),
    clientName: new FormControl('', { nonNullable: true, validators: Validators.required }),
    clientReference: new FormControl('', { nonNullable: true }),
    notes: new FormControl('', { nonNullable: true }),
  });

  private readonly quoteParams = toSignal(
    this.form.controls.stay.valueChanges.pipe(
      debounceTime(QUOTE_DEBOUNCE_MS),
      map(() => this.readQuoteParams()),
    ),
  );

  protected readonly quote = rxResource({
    params: this.quoteParams,
    stream: ({ params }) =>
      this.bookingsApi.quote(params.roomTypeId, params.checkIn, params.checkOut, params.rooms),
  });

  protected readonly quoteError = computed(() => {
    const error = this.quote.error();
    return error instanceof HttpErrorResponse ? toUserMessage(error) : undefined;
  });

  protected readonly quotedStay = computed(() => {
    const params = this.quoteParams();
    if (!params) return '';

    const nights = differenceInCalendarDays(parseISO(params.checkOut), parseISO(params.checkIn));
    return `${countOf(nights, 'noche', 'noches')} × ${countOf(params.rooms, 'habitación', 'habitaciones')}`;
  });

  protected confirm(): void {
    const params = this.quoteParams();
    // The quote is debounced, so the form can already hold a stay that has no price yet.
    if (
      this.form.invalid ||
      !params ||
      !isSameStay(params, this.readQuoteParams()) ||
      !this.quote.hasValue() ||
      this.quote.isLoading()
    ) {
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.dialogRef.disableClose = true;
    this.bookingsApi
      .create({
        ...params,
        adults: value.adults,
        children: value.children,
        leadGuestName: value.leadGuestName,
        channel: value.channel,
        clientName: value.clientName,
        clientReference: value.clientReference,
        notes: value.notes,
        quotedTotal: this.quote.value().totalPrice,
      })
      .subscribe({
        next: (booking) => this.dialogRef.close(booking),
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.dialogRef.disableClose = false;
          if (error.status === HttpStatusCode.Conflict) this.quote.reload();
        },
      });
  }

  private readQuoteParams(): QuoteParams | undefined {
    const stay = this.form.controls.stay;
    const { roomTypeId, checkIn, checkOut, rooms } = stay.getRawValue();
    if (stay.invalid || !checkIn || !checkOut) return undefined;

    return { roomTypeId, checkIn: toIsoDate(checkIn), checkOut: toIsoDate(checkOut), rooms };
  }
}

function isSameStay(quoted: QuoteParams, current: QuoteParams | undefined): boolean {
  return (
    quoted.roomTypeId === current?.roomTypeId &&
    quoted.checkIn === current.checkIn &&
    quoted.checkOut === current.checkOut &&
    quoted.rooms === current.rooms
  );
}

function countOf(count: number, one: string, other: string): string {
  return `${count} ${count === 1 ? one : other}`;
}
