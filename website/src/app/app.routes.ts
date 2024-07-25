import { Routes } from '@angular/router';
import { TableComponent } from './table/table.component'
import { HowItWorksComponent } from './how-it-works/how-it-works.component'

export const routes: Routes = [
  { path: 'elo-table', component: TableComponent },
  { path: 'how-it-works', component: HowItWorksComponent },
];
