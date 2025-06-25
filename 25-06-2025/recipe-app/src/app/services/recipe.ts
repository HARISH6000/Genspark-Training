import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Recipe } from '../models/recipe';

@Injectable({
  providedIn: 'root',
})
export class RecipeService {
  private apiUrl = 'https://dummyjson.com/recipes';

  constructor(private http: HttpClient) {}

  getRecipes(): Observable<{ recipes: Recipe[] }> {
    return this.http.get<{ recipes: Recipe[] }>(this.apiUrl).pipe(
      map((data: { recipes: Recipe[] }) => {
        return {
          recipes: data.recipes.map((recipe) => ({
            ...recipe,
            
            ingredients: recipe.ingredients, 
          })),
        };
      })
    );
  }
}
