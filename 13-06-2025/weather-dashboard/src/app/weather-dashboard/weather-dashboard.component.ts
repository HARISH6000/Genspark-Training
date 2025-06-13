import { Component } from '@angular/core';
import { AsyncPipe, NgIf } from '@angular/common';
import { Observable } from 'rxjs';
import { WeatherService, WeatherData } from '../weather.service';
import { CitySearchComponent } from '../city-search/city-search.component';
import { WeatherCardComponent } from '../weather-card/weather-card.component';

@Component({
  selector: 'app-weather-dashboard',
  standalone: true,
  imports: [CitySearchComponent, WeatherCardComponent, AsyncPipe, NgIf],
  templateUrl: './weather-dashboard.component.html',
  styleUrls: ['./weather-dashboard.component.css']
})
export class WeatherDashboardComponent {
  weather$!: Observable<WeatherData | null>;
  error$!: Observable<string>;

  constructor(private weatherService: WeatherService) {
    this.weather$ = this.weatherService.weather$;
    this.error$ = this.weatherService.error$;
  }
}