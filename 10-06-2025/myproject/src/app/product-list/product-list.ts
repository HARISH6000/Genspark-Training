import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; 

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.html',
  styleUrls: ['./product-list.css'],
  standalone: true,
  imports: [CommonModule]
})
export class ProductList {
  products = [
    { id: 1, name: 'Product A', price: 100, imageUrl: 'assets/legion3.jpg' },
    { id: 2, name: 'Product B', price: 150, imageUrl: 'assets/legion3.jpg' },
    { id: 3, name: 'Product C', price: 200, imageUrl: 'assets/legion3.jpg' }
  ];


  cartCount: number = 0;

  addToCart() {
    this.cartCount++;
  }
}