// frontend/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { ContractsPageComponent } from './features/contracts/contracts-page/contracts-page';

export const routes: Routes = [
  { path: '', redirectTo: 'contracts', pathMatch: 'full' },
  { path: 'contracts', component: ContractsPageComponent },
];
