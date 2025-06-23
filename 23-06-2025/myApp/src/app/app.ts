import { Component } from '@angular/core';
import { First } from "./first/first";

import { Products } from './products/products';
import { Menu } from "./menu/menu";
import { Login } from "./login/login";
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [ Menu,RouterOutlet]
})
export class App {
  protected title = 'myApp';
}
