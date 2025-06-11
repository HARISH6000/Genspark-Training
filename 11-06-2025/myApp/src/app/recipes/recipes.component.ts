import { Component, OnInit, signal } from '@angular/core';
import { RecipeService } from '../services/recipie.service';
import { RecipeModel } from '../models/recipe.model';       
import { RecipeComponent } from '../recipe/recipe.component'; 
import { CommonModule } from '@angular/common'; 

@Component({
  selector: 'app-recipes',
  standalone: true, 
  imports: [CommonModule, RecipeComponent], 
  templateUrl: './recipes.component.html',
  styleUrl: './recipes.component.css' 
})
export class RecipesComponent implements OnInit {
  
  recipes = signal<RecipeModel[]>([]);

  constructor(private recipeService: RecipeService) {} 

  ngOnInit(): void {
    
    this.recipeService.getAllRecipes().subscribe(
      {
        next:(data:any)=>{
         this.recipes.set(data.recipes as RecipeModel[]);
        },
        error:(err)=>{
            console.error('Error fetching recipes:', err);
            this.recipes.set([]); 
        },
        complete:()=>{
            console.log("Recipe fetching complete");
        }
      }
    );
  }
}
