import { bootstrapApplication } from '@angular/platform-browser';
   import { provideHttpClient } from '@angular/common/http';
   import { AppComponent } from './app/app';
   import { importProvidersFrom } from '@angular/core';
   import { AppModule } from './app/app-module';

   bootstrapApplication(AppComponent, {
     providers: [
       provideHttpClient(),
       importProvidersFrom(AppModule)
     ]
   }).catch(err => console.error(err));