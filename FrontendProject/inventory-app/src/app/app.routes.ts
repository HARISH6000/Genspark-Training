import { Routes } from '@angular/router';
import { LoginComponent } from './login/login';
import { AdminDashboardComponent } from './admin/dashboard/dashboard';
import { ManagerDashboardComponent } from './manager/dashboard/dashboard';
import { NotFoundComponent } from './shared/not-found/not-found';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';
import { map } from 'rxjs/operators';
import { CanActivateFn, Router } from '@angular/router';

// Auth Guard to check if user is authenticated
const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Check if token exists or currentUser data exists (from localStorage on refresh)
  if (authService.accessToken && authService.currentUserValue) {
    return true;
  }

  // If not authenticated, redirect to login page
  router.navigate(['/login']);
  return false;
};

// Admin Guard to check if user is admin
const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Ensure user data is loaded (important for initial load)
  return authService.currentUser.pipe(
    map(user => {
      if (user && authService.isAdmin()) {
        return true;
      }
      // Not admin or no user, redirect to appropriate place
      router.navigate(['/access-denied']); // Or back to login, or generic dashboard
      return false;
    })
  );
};

// Manager Guard to check if user is manager
const managerGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Ensure user data is loaded (important for initial load)
  return authService.currentUser.pipe(
    map(user => {
      if (user && authService.isManager()) {
        return true;
      }
      // Not manager or no user, redirect to appropriate place
      router.navigate(['/access-denied']); // Or back to login, or generic dashboard
      return false;
    })
  );
};

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: 'admin/dashboard',
    component: AdminDashboardComponent,
    canActivate: [authGuard, adminGuard] // Apply both auth and role guard
  },
  {
    path: 'manager/dashboard',
    component: ManagerDashboardComponent,
    canActivate: [authGuard, managerGuard] // Apply both auth and role guard
  },
  { path: '', redirectTo: '/login', pathMatch: 'full' }, // Redirect root to login
  { path: 'access-denied', component: NotFoundComponent, data: { message: 'Access Denied: You do not have permission to view this page.' } }, // Generic access denied page
  { path: '**', component: NotFoundComponent } // Wildcard for 404 Not Found
];