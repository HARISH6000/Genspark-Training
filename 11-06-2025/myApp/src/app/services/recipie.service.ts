import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { RecipeModel } from "../models/recipe.model"; 

@Injectable({
  providedIn: 'root' 
})
export class RecipeService {
    private http = inject(HttpClient);

   
    getAllRecipes(): Observable<{ recipes: RecipeModel[] }> {
        return this.http.get<{ recipes: RecipeModel[] }>('https://dummyjson.com/recipes');
    }
}
