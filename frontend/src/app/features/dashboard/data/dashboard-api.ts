import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SHOW_ERROR_SNACK_BAR } from '../../../core/http/http-error.interceptor';
import { DashboardSummaryResponse } from './dashboard-summary-response';
import { OccupancyDayResponse } from './occupancy-day-response';

// The dashboard shows a failed load in place of each panel, so no snack bar for these.
@Injectable({ providedIn: 'root' })
export class DashboardApi {
  private readonly http = inject(HttpClient);
  private readonly context = new HttpContext().set(SHOW_ERROR_SNACK_BAR, false);

  getSummary(date: string): Observable<DashboardSummaryResponse> {
    const params = new HttpParams().set('date', date);
    return this.http.get<DashboardSummaryResponse>('/api/dashboard/summary', {
      params,
      context: this.context,
    });
  }

  getOccupancy(from: string, to: string): Observable<OccupancyDayResponse[]> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<OccupancyDayResponse[]>('/api/dashboard/occupancy', {
      params,
      context: this.context,
    });
  }
}
