import { Component, OnInit, Input } from '@angular/core';
import { CommonModule, NgIf, NgFor } from '@angular/common';
import { ProductService } from '../services/product';
import { switchMap, catchError } from 'rxjs/operators';
import { of } from 'rxjs'; 
import { Router } from '@angular/router';

interface Product {
  id: number;
  title: string;
  description: string;
  price: number;
  discountPercentage: number;
  rating: number;
  stock: number;
  brand: string;
  category: string;
  thumbnail: string;
  images: string[];
}

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, NgIf, NgFor], 
  templateUrl: './product-detail.html',
  styleUrls: ['./product-detail.css']
})
export class ProductDetailComponent implements OnInit {
  @Input() id!: number; 
  product: Product | undefined; 
  loading: boolean = false; 
  errorMessage: string | null = null;

  constructor(private productService: ProductService, private router: Router) { }

  ngOnInit(): void {
    if (this.id) {
      this.fetchProductDetails(this.id);
    } else {
      this.errorMessage = 'Product ID not provided.';
    }
  }


  fetchProductDetails(id: number): void {
    this.loading = true;
    this.errorMessage = null; // Clear previous errors

    this.productService.getProductById(id).pipe(
      catchError(error => {
        console.error('Error fetching product details:', error);
        
        if (error.status === 404) {
          this.errorMessage = 'Product not found. Invalid ID.';
        } else {
          this.errorMessage = 'Failed to load product details. Please try again.';
        }
        this.loading = false;
        return of(undefined);
      })
    ).subscribe(product => {
      this.loading = false;
      if (product) {
        this.product = product;
      } else {
        this.product = undefined;
      }
    });
  }

  
  goBackToList(): void {
    this.router.navigate(['/products']);
  }
}