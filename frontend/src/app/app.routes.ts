import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', loadChildren: () => import('./features/dashboard/dashboard.routes') },
  { path: 'hotels', loadChildren: () => import('./features/hotels/hotels.routes') },
  { path: '**', redirectTo: 'dashboard' },
];
