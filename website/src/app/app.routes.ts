import { Routes } from '@angular/router';
import { TableComponent } from './table/table.component';
import { HowItWorksComponent } from './how-it-works/how-it-works.component';

export const routes: Routes = [
  { path: '', component: TableComponent },
  { path: 'how-it-works', component: HowItWorksComponent },
];
