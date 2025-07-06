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
import { CategoryManagementComponent } from './admin/category-management/category-management';
import { InventoryManagementComponent } from './admin/inventory-management/inventory-management';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';
import { map } from 'rxjs/operators';
import { CanActivateFn, Router } from '@angular/router';
import { ProductInfoComponent } from './products/product-info/product-info';
import { InventoryInfoComponent } from './shared/inventory-info/inventory-info';
import { NotificationListComponent } from './shared/notification-list/notification-list';
import { LandingPageComponent } from './landing-page/landing-page';


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
  { path: 'edit-user/:userId', 
    component: EditUserComponent,
    canActivate:[authGuard]
  },
  { path: 'products',
    component: ProductManagementComponent,
    canActivate:[authGuard]
  },
  { path: 'product-info/:id',
    component: ProductInfoComponent,
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
  {
    path:'category',
    component:CategoryManagementComponent,
    canActivate:[authGuard]
  },
  { path: 'inventories',
    component: InventoryManagementComponent,
    canActivate:[authGuard]
  },
  { path: 'inventory/:id',
    component: InventoryInfoComponent,
    canActivate:[authGuard]
  },
  {
    path: 'notifications', 
    component: NotificationListComponent,
    canActivate: [authGuard]
  },
  { path: '', component: LandingPageComponent, pathMatch: 'full' },
  { path: 'access-denied', component: NotFoundComponent, data: { message: 'Access Denied: You do not have permission to view this page.' } },
  { path: '**', component: NotFoundComponent }
];