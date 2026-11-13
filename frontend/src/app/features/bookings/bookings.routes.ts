import { Routes } from '@angular/router';
import { BookingDetailPage } from './booking-detail-page/booking-detail-page';

const BOOKINGS_ROUTES: Routes = [
  { path: ':id', title: 'Reserva · Altairis', component: BookingDetailPage },
];

export default BOOKINGS_ROUTES;
