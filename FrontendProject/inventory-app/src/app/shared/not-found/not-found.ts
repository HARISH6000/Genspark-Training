import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule],
  template: `<div class="d-flex justify-content-center align-items-center min-vh-100" style="background-color: var(--color-primary-bg);">
               <div class="text-center">
                 <h1 class="display-1" style="color: var(--color-accent);">404</h1>
                 <h2 style="color: var(--color-primary-text);">Page Not Found</h2>
                 <p style="color: var(--color-secondary-text);">The page you are looking for does not exist.</p>
                 <a routerLink="/login" class="btn mt-3" style="background-color: var(--color-accent); color: var(--color-content-bg);">Go to Login</a>
               </div>
             </div>`,
  styles: []
})
export class NotFoundComponent {}