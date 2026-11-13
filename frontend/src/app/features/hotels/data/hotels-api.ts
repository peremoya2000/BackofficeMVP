import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResponse } from '../../../core/http/paged-response';
import { CreateHotelRequest } from './create-hotel-request';
import { HotelListItemResponse } from './hotel-list-item-response';
import { HotelResponse } from './hotel-response';
import { HotelSearchRequest } from './hotel-search-request';

const HOTELS_URL = '/api/hotels';

@Injectable({ providedIn: 'root' })
export class HotelsApi {
  private readonly http = inject(HttpClient);

  search(request: HotelSearchRequest): Observable<PagedResponse<HotelListItemResponse>> {
    let params = new HttpParams().set('page', request.page).set('pageSize', request.pageSize);
    if (request.sort) params = params.set('sort', request.sort);
    if (request.search) params = params.set('search', request.search);
    if (request.countryCode) params = params.set('countryCode', request.countryCode);
    if (request.stars) params = params.set('stars', request.stars);

    return this.http.get<PagedResponse<HotelListItemResponse>>(HOTELS_URL, { params });
  }

  getById(id: string): Observable<HotelResponse> {
    return this.http.get<HotelResponse>(`${HOTELS_URL}/${id}`);
  }

  create(request: CreateHotelRequest): Observable<HotelResponse> {
    return this.http.post<HotelResponse>(HOTELS_URL, request);
  }
}
