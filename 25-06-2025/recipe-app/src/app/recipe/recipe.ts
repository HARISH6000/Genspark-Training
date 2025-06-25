import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RecipeService } from '../services/recipe';
import { Recipe } from '../models/recipe';
import { RecipeCardComponent } from '../recipe-card/recipe-card';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-recipe',
  standalone: true,
  imports: [CommonModule, RecipeCardComponent, HttpClientModule],
  templateUrl: './recipe.html',
  styleUrls: ['./recipe.css'],
  providers: [RecipeService]
})
export class RecipeComponent implements OnInit {
  recipes: Recipe[] = [];
  loading = true;

  constructor(private recipeService: RecipeService) {}

  ngOnInit(): void {
    this.recipeService.getRecipes().subscribe({
      next: (data) => {
        this.recipes = data.recipes;
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        console.error('Failed to load recipes:', error);
        this.displayErrorMessage('Failed to load recipes. Please try again later.');
      },
    });
  }

  private displayErrorMessage(message: string): void {
    const errorDiv = document.createElement('div');
    errorDiv.className = 'fixed inset-0 bg-red-500 bg-opacity-75 flex items-center justify-center z-50';
    errorDiv.innerHTML = `
      <div class="bg-white p-6 rounded-lg shadow-xl text-center">
        <p class="text-red-700 text-lg mb-4">${message}</p>
        <button class="bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded" onclick="this.parentElement.parentElement.remove()">
          Close
        </button>
      </div>
    `;
    document.body.appendChild(errorDiv);
  }
}
