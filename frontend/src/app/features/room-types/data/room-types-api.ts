import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SHOW_ERROR_SNACK_BAR } from '../../../core/http/http-error.interceptor';
import { CreateRoomTypeRequest } from './create-room-type-request';
import { RoomTypeResponse } from './room-type-response';

@Injectable({ providedIn: 'root' })
export class RoomTypesApi {
  private readonly http = inject(HttpClient);

  list(hotelId: string): Observable<RoomTypeResponse[]> {
    // The section shows a failed load in place of the table, so no snack bar for it.
    const context = new HttpContext().set(SHOW_ERROR_SNACK_BAR, false);
    return this.http.get<RoomTypeResponse[]>(roomTypesUrl(hotelId), { context });
  }

  create(hotelId: string, request: CreateRoomTypeRequest): Observable<RoomTypeResponse> {
    return this.http.post<RoomTypeResponse>(roomTypesUrl(hotelId), request);
  }
}

function roomTypesUrl(hotelId: string): string {
  return `/api/hotels/${hotelId}/room-types`;
}
