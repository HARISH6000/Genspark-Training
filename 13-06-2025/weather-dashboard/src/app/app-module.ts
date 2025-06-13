import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { AppComponent } from './app';
import { WeatherDashboardComponent } from './weather-dashboard/weather-dashboard.component';
import { CitySearchComponent } from './city-search/city-search.component';
import { WeatherCardComponent } from './weather-card/weather-card.component';

@NgModule({
  imports: [
    BrowserModule,
    FormsModule,
    AppComponent,
    WeatherDashboardComponent,
    CitySearchComponent,
    WeatherCardComponent
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }