import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SHOW_ERROR_SNACK_BAR } from '../../../core/http/http-error.interceptor';
import { InventoryCalendarResponse } from './inventory-calendar-response';
import { UpdateInventoryRangeRequest } from './update-inventory-range-request';

@Injectable({ providedIn: 'root' })
export class InventoryApi {
  private readonly http = inject(HttpClient);

  // The grid shows a failed load in place of the calendar, so no snack bar for it.
  getCalendar(hotelId: string, from: string, to: string): Observable<InventoryCalendarResponse> {
    const params = new HttpParams().set('from', from).set('to', to);
    const context = new HttpContext().set(SHOW_ERROR_SNACK_BAR, false);
    return this.http.get<InventoryCalendarResponse>(`/api/hotels/${hotelId}/inventory`, {
      params,
      context,
    });
  }

  updateRange(roomTypeId: string, request: UpdateInventoryRangeRequest): Observable<void> {
    return this.http.put<void>(`/api/room-types/${roomTypeId}/inventory`, request);
  }
}
