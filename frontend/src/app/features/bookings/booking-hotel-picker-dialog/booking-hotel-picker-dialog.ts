import { Component, computed, inject } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { debounceTime, distinctUntilChanged, map } from 'rxjs';
import { LoadError } from '../../../shared/load-error/load-error';
import { HotelListItemResponse } from '../../hotels/data/hotel-list-item-response';
import { HotelsApi } from '../../hotels/data/hotels-api';
import { RoomTypesApi } from '../../room-types/data/room-types-api';
import { BookingCreateDialogData } from '../booking-create-dialog/booking-create-dialog';

const SEARCH_DEBOUNCE_MS = 300;
const MIN_SEARCH_LENGTH = 2;
const SUGGESTION_COUNT = 10;

@Component({
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
    LoadError,
  ],
  selector: 'app-booking-hotel-picker-dialog',
  styleUrl: './booking-hotel-picker-dialog.scss',
  templateUrl: './booking-hotel-picker-dialog.html',
})
export class BookingHotelPickerDialog {
  private readonly dialogRef =
    inject<MatDialogRef<BookingHotelPickerDialog, BookingCreateDialogData>>(MatDialogRef);
  private readonly hotelsApi = inject(HotelsApi);
  private readonly roomTypesApi = inject(RoomTypesApi);

  // Holds the typed text until an option is picked, then the picked hotel.
  protected readonly hotel = new FormControl<HotelListItemResponse | string>('', {
    nonNullable: true,
  });

  private readonly search = toSignal(
    this.hotel.valueChanges.pipe(
      map((value) => (typeof value === 'string' ? value.trim() : '')),
      debounceTime(SEARCH_DEBOUNCE_MS),
      distinctUntilChanged(),
    ),
    { initialValue: '' },
  );

  protected readonly selectedHotel = toSignal(
    this.hotel.valueChanges.pipe(map((value) => (typeof value === 'string' ? undefined : value))),
  );

  protected readonly suggestions = rxResource({
    params: () => (this.search().length >= MIN_SEARCH_LENGTH ? this.search() : undefined),
    stream: ({ params: search }) =>
      this.hotelsApi
        .search({ search, page: 1, pageSize: SUGGESTION_COUNT })
        .pipe(map((page) => page.items)),
  });

  protected readonly roomTypes = rxResource({
    params: () => this.selectedHotel()?.id,
    stream: ({ params: hotelId }) => this.roomTypesApi.list(hotelId),
  });

  protected readonly options = computed(() =>
    this.suggestions.hasValue() ? this.suggestions.value() : [],
  );

  protected readonly roomTypeOptions = computed(() =>
    this.roomTypes.hasValue()
      ? this.roomTypes.value().map((roomType) => ({ id: roomType.id, name: roomType.name }))
      : [],
  );

  protected hotelName(hotel: HotelListItemResponse | string): string {
    return typeof hotel === 'string' ? hotel : hotel.name;
  }

  protected continue(): void {
    const hotel = this.selectedHotel();
    if (!hotel || this.roomTypeOptions().length === 0) return;

    this.dialogRef.close({ hotelName: hotel.name, roomTypes: this.roomTypeOptions() });
  }
}
