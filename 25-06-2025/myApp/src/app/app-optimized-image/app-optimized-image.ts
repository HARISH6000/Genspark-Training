import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-optimized-image-demo',
  standalone: true,
  imports: [CommonModule, NgOptimizedImage],
  template: `
    <div class="image-container">
      <h2>NgOptimizedImage Demo</h2>
      <img
        ngSrc="https://img.freepik.com/free-photo/vestrahorn-mountains-stokksnes-iceland_335224-667.jpg?semt=ais_hybrid&w=740"
        width="800"
        height="400"
        alt="Placeholder Image"
        priority
        class="optimized-image"
      />
      <p>Above image is optimized using Angular's NgOptimizedImage.</p>
    </div>
  `,
  styles: [
    `
      .image-container {
        text-align: center;
        margin: 2rem;
      }
      .optimized-image {
        border: 2px solid #007bff;
        border-radius: 10px;
      }
    `,
  ],
})
export class OptimizedImageDemoComponent {}
