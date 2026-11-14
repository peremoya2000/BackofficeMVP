import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SHOW_ERROR_SNACK_BAR } from '../../../core/http/http-error.interceptor';
import { PagedResponse } from '../../../core/http/paged-response';
import { BookingListItemResponse } from './booking-list-item-response';
import { BookingResponse } from './booking-response';
import { BookingSearchRequest } from './booking-search-request';
import { CreateBookingRequest } from './create-booking-request';
import { QuoteResponse } from './quote-response';

const BOOKINGS_URL = '/api/bookings';

@Injectable({ providedIn: 'root' })
export class BookingsApi {
  private readonly http = inject(HttpClient);

  search(request: BookingSearchRequest): Observable<PagedResponse<BookingListItemResponse>> {
    let params = new HttpParams().set('page', request.page).set('pageSize', request.pageSize);
    if (request.sort) params = params.set('sort', request.sort);
    if (request.search) params = params.set('search', request.search);
    if (request.status) params = params.set('status', request.status);
    if (request.checkIn) params = params.set('checkIn', request.checkIn);
    if (request.checkOut) params = params.set('checkOut', request.checkOut);

    // The list shows a failed load in place of the table, so no snack bar for it.
    const context = new HttpContext().set(SHOW_ERROR_SNACK_BAR, false);
    return this.http.get<PagedResponse<BookingListItemResponse>>(BOOKINGS_URL, { params, context });
  }

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
