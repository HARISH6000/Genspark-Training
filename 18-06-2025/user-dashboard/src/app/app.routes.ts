import { Routes } from '@angular/router';
import { UserDashboardComponent } from './components/user-dashboard/user-dashboard.component';
import { AddUserComponent } from './components/add-user/add-user.component';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: UserDashboardComponent },
  {path:'adduser',component:AddUserComponent}
];