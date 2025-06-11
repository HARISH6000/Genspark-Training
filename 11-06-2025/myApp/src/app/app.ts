import { Component } from '@angular/core';
import { First } from "./first/first";

import { Products } from './products/products';
import { RecipesComponent } from './recipes/recipes.component';
@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [ Products, RecipesComponent]
})
export class App {
  protected title = 'myApp';
}
