import { bootstrapApplication } from '@angular/platform-browser';
import { App} from './app/app';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, Routes } from '@angular/router';
import { Home } from './app/home/home';
import { About} from './app/about/about';


const routes: Routes = [
  { path: 'home', component: Home },
  { path: 'about', component: About },
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: '**', redirectTo: '/home', pathMatch: 'full' }
];

bootstrapApplication(App, {
  providers: [
    provideHttpClient(),
    provideRouter(routes) 
  ]
}).catch(err => console.error(err));

