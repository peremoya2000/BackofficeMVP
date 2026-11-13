import { Routes } from '@angular/router';
import { HotelCreatePage } from './hotel-create-page/hotel-create-page';
import { HotelDetailPage } from './hotel-detail-page/hotel-detail-page';
import { HotelListPage } from './hotel-list-page/hotel-list-page';

const HOTELS_ROUTES: Routes = [
  { path: '', title: 'Hoteles · Altairis', component: HotelListPage },
  { path: 'new', title: 'Nuevo hotel · Altairis', component: HotelCreatePage },
  { path: ':id', title: 'Hotel · Altairis', component: HotelDetailPage },
];

export default HOTELS_ROUTES;
