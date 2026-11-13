import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SHOW_ERROR_SNACK_BAR } from '../../../core/http/http-error.interceptor';
import { BookingResponse } from './booking-response';
import { CreateBookingRequest } from './create-booking-request';
import { QuoteResponse } from './quote-response';

const BOOKINGS_URL = '/api/bookings';

@Injectable({ providedIn: 'root' })
export class BookingsApi {
  private readonly http = inject(HttpClient);

  // The form shows why a stay can't be sold next to the price, so no snack bar for it.
  quote(
    roomTypeId: string,
    checkIn: string,
    checkOut: string,
    rooms: number,
  ): Observable<QuoteResponse> {
    const params = new HttpParams()
      .set('checkIn', checkIn)
      .set('checkOut', checkOut)
      .set('rooms', rooms);
    const context = new HttpContext().set(SHOW_ERROR_SNACK_BAR, false);
    return this.http.get<QuoteResponse>(`/api/room-types/${roomTypeId}/quote`, { params, context });
  }

  create(request: CreateBookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(BOOKINGS_URL, request);
  }

  getById(id: string): Observable<BookingResponse> {
    return this.http.get<BookingResponse>(`${BOOKINGS_URL}/${id}`);
  }

  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${BOOKINGS_URL}/${id}/cancel`, null);
  }
}
