import { DatePipe } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CountryNamePipe } from '../../../shared/countries/country-name-pipe';
import { PageHeader } from '../../../shared/page-header/page-header';
import { Stars } from '../../../shared/stars/stars';
import { HotelsApi } from '../data/hotels-api';

@Component({
  imports: [DatePipe, MatCardModule, MatProgressBarModule, CountryNamePipe, PageHeader, Stars],
  selector: 'app-hotel-detail-page',
  styleUrl: './hotel-detail-page.scss',
  templateUrl: './hotel-detail-page.html',
})
export class HotelDetailPage {
  private readonly hotelsApi = inject(HotelsApi);

  readonly id = input.required<string>();

  protected readonly hotel = rxResource({
    params: this.id,
    stream: ({ params: id }) => this.hotelsApi.getById(id),
  });
}
