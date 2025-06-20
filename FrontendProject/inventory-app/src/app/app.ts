import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common'; // Needed for RouterOutlet in standalone

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule], // Add RouterOutlet and CommonModule here
  template: `
    <!-- The router-outlet is where matched components will be displayed -->
    <router-outlet></router-outlet>
  `,
  styleUrl: './app.css'
})
export class App {
  title = 'inventory-app';
}