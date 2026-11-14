import { DatePipe, DecimalPipe, PercentPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RouterLink } from '@angular/router';
import { addDays, startOfToday } from 'date-fns';
import { toIsoDate } from '../../../shared/dates/iso-date';
import { LoadError } from '../../../shared/load-error/load-error';
import { PageHeader } from '../../../shared/page-header/page-header';
import { DashboardApi } from '../data/dashboard-api';
import { OccupancyChart } from '../occupancy-chart/occupancy-chart';

const OCCUPANCY_NIGHTS = 30;

@Component({
  imports: [
    DatePipe,
    DecimalPipe,
    PercentPipe,
    RouterLink,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    LoadError,
    PageHeader,
    OccupancyChart,
  ],
  selector: 'app-dashboard-page',
  styleUrl: './dashboard-page.scss',
  templateUrl: './dashboard-page.html',
})
export class DashboardPage {
  private readonly dashboardApi = inject(DashboardApi);

  protected readonly occupancyNights = OCCUPANCY_NIGHTS;
  protected readonly today = toIsoDate(startOfToday());

  protected readonly summary = rxResource({
    stream: () => this.dashboardApi.getSummary(this.today),
  });

  protected readonly occupancy = rxResource({
    stream: () =>
      this.dashboardApi.getOccupancy(
        this.today,
        toIsoDate(addDays(startOfToday(), OCCUPANCY_NIGHTS - 1)),
      ),
  });
}
