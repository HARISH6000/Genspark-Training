import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `<div class="p-4">
               <h2 class="text-center" style="color: var(--color-primary-text);">Admin Dashboard</h2>
               <p class="text-center" style="color: var(--color-secondary-text);">Welcome, Admin! This is your dashboard!</p>
             </div>`,
  styles: [`
    :host {
      display: block;
      min-height: calc(100vh - 56px); /* Adjust based on your header height */
      background-color: var(--color-primary-bg);
    }
  `]
})
export class AdminDashboardComponent {}