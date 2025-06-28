// src/app/components/product-add-edit/product-add-edit.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { AddProductRequest, UpdateProductRequest, Product } from '../../models/product';
import { Category } from '../../models/inventory'; // Assuming Category is also in inventory models or create a separate category model
import { of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

@Component({
  selector: 'app-product-add-edit',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-add-edit.html',
  styleUrls: ['./product-add-edit.css']
})
export class ProductAddEditComponent implements OnInit {
  productId: number | null = null;
  product: AddProductRequest | UpdateProductRequest = {
    sku: '',
    productName: '',
    description: '',
    unitPrice: 0,
    categoryId: 0
  };
  categories: Category[] = [];
  loading = false;
  errorMessage: string | null = null;
  isEditMode = false;
  successMessage: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    this.fetchCategories();
    this.productId = this.route.snapshot.paramMap.get('id') ? Number(this.route.snapshot.paramMap.get('id')) : null;
    this.isEditMode = !!this.productId;

    if (this.isEditMode && this.productId) {
      this.fetchProductDetails(this.productId);
    }
  }

  fetchCategories(): void {
    this.categoryService.getAllCategories().pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to load categories.';
        return of([]);
      })
    ).subscribe(categories => {
      this.categories = categories;
    });
  }

  fetchProductDetails(id: number): void {
    this.loading = true;
    this.productService.getProductById(id).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to load product details.';
        this.loading = false;
        return of(null);
      }),
      finalize(() => this.loading = false)
    ).subscribe(product => {
      if (product) {
        this.product = {
          productId: product.productId,
          sku: product.sku,
          productName: product.productName,
          description: product.description,
          unitPrice: product.unitPrice,
          categoryId: product.categoryId
        } as UpdateProductRequest; // Cast to UpdateProductRequest
      } else {
        this.router.navigate(['/products']); // Redirect if product not found
      }
    });
  }

  onSubmit(): void {
    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;

    if (this.isEditMode) {
      // Update existing product
      const updateRequest: UpdateProductRequest = this.product as UpdateProductRequest;
      this.productService.updateProduct(updateRequest).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to update product.';
          this.loading = false;
          return of(null);
        }),
        finalize(() => this.loading = false)
      ).subscribe(response => {
        if (response) {
          this.successMessage = 'Product updated successfully!';
          this.router.navigate(['/products']); // Navigate back to product list
        }
      });
    } else {
      // Add new product
      const addRequest: AddProductRequest = this.product as AddProductRequest;
      this.productService.addProduct(addRequest).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to add product.';
          this.loading = false;
          return of(null);
        }),
        finalize(() => this.loading = false)
      ).subscribe(response => {
        if (response) {
          this.successMessage = 'Product added successfully!';
          this.router.navigate(['/products']); // Navigate back to product list
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/products']);
  }
}