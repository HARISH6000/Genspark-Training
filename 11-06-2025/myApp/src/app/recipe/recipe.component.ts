import { Component, Input, signal } from '@angular/core';
import { RecipeModel } from '../models/recipe.model';
import { CommonModule } from '@angular/common'; 

@Component({
  selector: 'app-recipe',
  standalone: true, 
  imports: [CommonModule], 
  templateUrl: './recipe.component.html',
  styleUrl: './recipe.component.css', 
})
export class RecipeComponent { 
  
  @Input() recipe: RecipeModel | undefined;

  constructor() {
  }
}
