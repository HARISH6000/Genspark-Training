import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, switchMap, throwError, timer } from 'rxjs';


export interface WeatherData {
  city: string;
  temperature: number;
  condition: string;
  humidity: number;
  windSpeed: number;
  icon: string;
}

@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  
  private weatherSubject = new BehaviorSubject<WeatherData | null>(null);
  weather$ = this.weatherSubject.asObservable();


  private errorSubject = new BehaviorSubject<string>('');
  error$ = this.errorSubject.asObservable();

  constructor(private http: HttpClient) {}

  fetchWeather(city: string, autoRefresh: boolean = false): void {
    // get coordinates for the city
    const geocodeUrl = `https://geocoding-api.open-meteo.com/v1/search?name=${encodeURIComponent(city)}&count=1`;
    
    this.http.get<any>(geocodeUrl).pipe(
      switchMap(geocode => {
        if (!geocode.results || geocode.results.length === 0) {
          throw new Error('City not found');
        }
        console.log(geocode.results);
        const { latitude, longitude, name } = geocode.results[0];

        //get weather with coordinates
        const weatherUrl = `https://api.open-meteo.com/v1/forecast?latitude=${latitude}&longitude=${longitude}&current=temperature_2m,relative_humidity_2m,wind_speed_10m,weather_code`;
        return this.http.get<any>(weatherUrl);
      }),
      catchError(error => {
        this.errorSubject.next(error.message || 'Failed to fetch weather data');
        this.weatherSubject.next(null);
        return throwError(() => error);
      })
    ).subscribe(weather => {
      
      const condition = this.mapWeatherCode(weather.current.weather_code);
      const weatherData: WeatherData = {
        city: city,
        temperature: Math.round(weather.current.temperature_2m),
        condition: condition.text,
        humidity: weather.current.relative_humidity_2m,
        windSpeed: weather.current.wind_speed_10m,
        icon: condition.icon
      };
      this.weatherSubject.next(weatherData);
      this.errorSubject.next('');

     
      if (autoRefresh) {
        timer(300000).subscribe(() => this.fetchWeather(city, true)); //5 mins
      }
    });
  }

  // Map Open-Meteo weather codes to readable conditions and Font Awesome icons
  private mapWeatherCode(code: number): { text: string, icon: string } {
    switch (code) {
      case 0: return { text: 'Clear Sky', icon: 'fas fa-sun' };
      case 1: case 2: case 3: return { text: 'Partly Cloudy', icon: 'fas fa-cloud-sun' };
      case 45: case 48: return { text: 'Fog', icon: 'fas fa-smog' };
      case 51: case 53: case 55: return { text: 'Drizzle', icon: 'fas fa-cloud-rain' };
      case 61: case 63: case 65: return { text: 'Rain', icon: 'fas fa-cloud-showers-heavy' };
      case 71: case 73: case 75: return { text: 'Snow', icon: 'fas fa-snowflake' };
      case 95: return { text: 'Thunderstorm', icon: 'fas fa-bolt' };
      default: return { text: 'Unknown', icon: 'fas fa-question' };
    }
  }
}