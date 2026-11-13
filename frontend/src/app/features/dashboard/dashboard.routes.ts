import { Routes } from '@angular/router';
import { DashboardPage } from './dashboard-page/dashboard-page';

const DASHBOARD_ROUTES: Routes = [
  { path: '', title: 'Panel · Altairis', component: DashboardPage },
];

export default DASHBOARD_ROUTES;
