import { DatePipe } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RouterLink } from '@angular/router';
import { CountryNamePipe } from '../../../shared/countries/country-name-pipe';
import { PageHeader } from '../../../shared/page-header/page-header';
import { Stars } from '../../../shared/stars/stars';
import { RoomTypesSection } from '../../room-types/room-types-section/room-types-section';
import { HotelsApi } from '../data/hotels-api';

@Component({
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule,
    CountryNamePipe,
    PageHeader,
    Stars,
    RoomTypesSection,
  ],
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
