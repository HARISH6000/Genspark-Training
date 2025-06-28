import { Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login';
import { AdminDashboardComponent } from './admin/dashboard/dashboard';
import { ManagerDashboardComponent } from './manager/dashboard/dashboard';
import { RegisterUserComponent } from './admin/register-user/register-user';
import { UserInfoComponent } from './users/user-info/user-info';
import { EditUserComponent } from './admin/edit-user/edit-user';
import { UserManagementComponent } from './admin/user-management/user-management';
import { NotFoundComponent } from './shared/not-found/not-found';
import { ProductManagementComponent } from './products/product-management/product-management';
import { ProductAddEditComponent } from './products/product-add-edit/product-add-edit';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';
import { map } from 'rxjs/operators';
import { CanActivateFn, Router } from '@angular/router';


const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  
  if (authService.accessToken && authService.currentUserValue) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};


const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.currentUser.pipe(
    map(user => {
      if (user && authService.isAdmin()) {
        return true;
      }
      
      router.navigate(['/access-denied']); 
      return false;
    })
  );
};


const managerGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  
  return authService.currentUser.pipe(
    map(user => {
      if (user && authService.isManager()) {
        return true;
      }

      router.navigate(['/access-denied']);
      return false;
    })
  );
};

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: 'admin/dashboard',
    component: AdminDashboardComponent,
    canActivate: [authGuard, adminGuard] 
  },
  {
    path: 'manager/dashboard',
    component: ManagerDashboardComponent,
    canActivate: [authGuard, managerGuard] 
  },
  {
    path: 'admin/register-user', 
    component: RegisterUserComponent,
    canActivate: [authGuard, adminGuard] 
  },
  {
    path: 'admin/users',
    component: UserManagementComponent,
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'user-info', 
    component: UserInfoComponent,
    canActivate: [authGuard] 
  },
  {
    path: 'user-info/:userId', 
    component: UserInfoComponent,
    canActivate: [authGuard,adminGuard] 
  },
  { path: 'admin/edit-user/:userId', 
    component: EditUserComponent,
    canActivate:[authGuard,adminGuard]
  },
  { path: 'products',
    component: ProductManagementComponent,
    canActivate:[authGuard]
  },
  { path: 'add-product',
    component: ProductAddEditComponent,
    canActivate:[authGuard]
  },
  { path: 'edit-product/:id',
    component: ProductAddEditComponent,
    canActivate:[authGuard]
  },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'access-denied', component: NotFoundComponent, data: { message: 'Access Denied: You do not have permission to view this page.' } },
  { path: '**', component: NotFoundComponent } // Wildcard for 404 Not Found
];