import { Routes } from '@angular/router';
import { BookingDetailPage } from './booking-detail-page/booking-detail-page';
import { BookingListPage } from './booking-list-page/booking-list-page';

const BOOKINGS_ROUTES: Routes = [
  { path: '', title: 'Reservas · Altairis', component: BookingListPage },
  { path: ':id', title: 'Reserva · Altairis', component: BookingDetailPage },
];

export default BOOKINGS_ROUTES;
