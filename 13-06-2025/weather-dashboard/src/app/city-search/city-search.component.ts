import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WeatherService } from '../weather.service';

@Component({
  selector: 'app-city-search',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './city-search.component.html',
  styleUrls: ['./city-search.component.css']
})
export class CitySearchComponent {
  city: string = '';

  constructor(private weatherService: WeatherService) {}

  onSearch(): void {
    if (this.city.trim()) {
      this.weatherService.fetchWeather(this.city, true);
      this.city = '';
    }
  }
}