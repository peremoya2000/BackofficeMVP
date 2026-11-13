import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InventoryCalendarResponse } from './inventory-calendar-response';
import { UpdateInventoryRangeRequest } from './update-inventory-range-request';

@Injectable({ providedIn: 'root' })
export class InventoryApi {
  private readonly http = inject(HttpClient);

  getCalendar(hotelId: string, from: string, to: string): Observable<InventoryCalendarResponse> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<InventoryCalendarResponse>(`/api/hotels/${hotelId}/inventory`, { params });
  }

  updateRange(roomTypeId: string, request: UpdateInventoryRangeRequest): Observable<void> {
    return this.http.put<void>(`/api/room-types/${roomTypeId}/inventory`, request);
  }
}
