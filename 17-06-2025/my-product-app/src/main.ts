import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Routes, withComponentInputBinding } from '@angular/router';
import { HomeComponent } from './app/home/home';
import { About } from './app/about/about';
import { LoginComponent } from './app/login/login';
import { ProductDetailComponent } from './app/product-detail/product-detail';
import { authGuard } from './app/auth-guard';


const routes: Routes = [
  {
    path: 'products', 
    component: HomeComponent,
    canActivate: [authGuard]
  },
  {
    path: 'products/:id',
    component: ProductDetailComponent,
    canActivate: [authGuard] 
  },
  {
    path: 'about',
    component: About
  },
  {
    path: 'login', 
    component: LoginComponent
  },
  { path: '', redirectTo: '/products', pathMatch: 'full' }, 
  { path: '**', redirectTo: '/products', pathMatch: 'full' }
];


bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(), 
    provideRouter(routes, withComponentInputBinding())
  ]
}).catch(err => console.error(err));

