import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common'; // Needed for RouterOutlet in standalone
import { NavbarComponent } from './shared/navbar/navbar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, NavbarComponent], // Add RouterOutlet and CommonModule here
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  title = 'inventory-app';
}