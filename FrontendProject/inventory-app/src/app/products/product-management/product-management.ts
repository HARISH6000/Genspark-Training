import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, combineLatest, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, startWith } from 'rxjs/operators';
import { Product, AddProductRequest, UpdateProductRequest } from '../../models/product';
import { PaginationResponse} from '../../models/pagination-response'; 
import { PageNumberComponent } from '../../shared/page-number/page-number';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../models/inventory';
import { AuthService } from '../../services/auth.service'; 
import { SortCriterion } from '../../models/sortCriterion'; 


@Component({
  selector: 'app-product-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, PageNumberComponent],
  templateUrl: './product-management.html',
  styleUrl: './product-management.css' 
})
export class ProductManagementComponent implements OnInit {
  products: Product[] = [];
  filteredProducts: Product[] = [];
  categories: Category[]=[]; 
  selectedCategory: number | null = null;
  showDeleted: boolean = false; 

  
  pageNumber: number = 1;
  pageSize: number = 10; 
  totalCount: number = 0;
  totalPages: number = 0;

  // Search & Loading/Error states
  searchControl = new FormControl('');
  loading: boolean = true;
  errorMessage: string | null = null;

  // Multi-level Sorting
  sortFields = [
    { value: 'productName', name: 'Product Name' },
    { value: 'sku', name: 'SKU' },
    { value: 'unitPrice', name: 'Unit Price' },
    { value: 'categoryName', name: 'Category' },
    { value: 'productId', name: 'Product ID' }
  ];
  sortOrders = [
    { value: 'asc', name: 'Ascending' },
    { value: 'desc', name: 'Descending' }
  ];
  sortCriteria: SortCriterion[] = [];

  constructor(
    private authService: AuthService,
    private productService: ProductService,
    private categoryService: CategoryService,
    private router: Router
  ) {}

  ngOnInit(): void {
    
    this.loadCategories(); 
    this.addSortCriterion();

    
    this.searchControl.valueChanges.pipe(
      debounceTime(300), 
      distinctUntilChanged(), 
      startWith('') 
    ).subscribe(searchTerm => {
      this.pageNumber = 1; 
      this.fetchProducts(searchTerm || '');
    });

  }

  loadCategories(): void {
    this.loading = true;
    this.categoryService.getAllCategories().pipe(catchError(error => {
        this.errorMessage = error.message || 'Failed to load categories.';
        return []; 
      })).subscribe(response=>{
        console.log("cat:", response);
        this.categories=response;
      });
  }

  fetchProducts(searchTerm: string | null = null): void {
    this.loading = true;
    this.errorMessage = null;

    const orderByParam = this.buildOrderByString();

    this.productService.getAllProducts(
      this.pageNumber,
      this.pageSize,
      searchTerm,
      orderByParam,
      this.showDeleted,
    ).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to load products.';
        this.loading = false;
        return of({ data: [], pagination:{ totalRecords:0, totalPages: 0} }); 
      })
    ).subscribe(response => {
      console.log("res:",response);
      this.products = response.data;
      this.totalCount = response.pagination.totalRecords;
      this.totalPages = response.pagination.totalPages;
      this.applyFilters();
      this.loading = false;
    });
  }


  applyFilters(): void {
    let tempProducts = [...this.products];
    //console.log("pro.catID:",tempProducts[0].categoryId,",catId:",this.selectedCategory);
    // Filter by category
    if (this.selectedCategory !== null && this.selectedCategory > 0) {
      tempProducts = tempProducts.filter(p => p.categoryId == this.selectedCategory);
    }
    
    // Filter by isDeleted (only if showDeleted is false) - Backend `includeDeleted` handles main filtering
    // This frontend filter is mostly redundant if backend handles includeDeleted true/false fully,
    // but useful if you fetch all and then toggle visibility
    if (!this.showDeleted) {
      tempProducts = tempProducts.filter(p => !p.isDeleted);
    }
    
    this.filteredProducts = tempProducts;

  }

  // --- Event Handlers for UI ---

  onCategoryChange(): void {
    this.pageNumber = 1; // Reset to first page on filter change
    this.fetchProducts(this.searchControl.value); // Re-fetch products with new category filter
  }

  onShowDeletedChange(): void {
    this.pageNumber = 1; // Reset to first page on filter change
    this.fetchProducts(this.searchControl.value); // Re-fetch products with new deleted status
  }

  // Pagination Handlers
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.pageNumber = page;
      this.fetchProducts(this.searchControl.value);
    }
  }

  onPageSizeChange(): void {
    this.pageNumber = 1; // Reset to first page when page size changes
    this.fetchProducts(this.searchControl.value);
  }

  // --- Dynamic Multi-level Sort Methods ---
  private buildOrderByString(): string {
    return this.sortCriteria
      .filter(c => c.field && c.order) // Only include valid criteria
      .map(c => `${c.field}_${c.order}`)
      .join(',');
  }

  addSortCriterion(): void {
    this.sortCriteria.push({ field: '', order: 'asc' });
  }

  removeSortCriterion(index: number): void {
    this.sortCriteria.splice(index, 1);
    this.onSortChange(); // Re-fetch data after removing a sort criterion
  }

  onSortChange(): void {
    this.pageNumber = 1; // Reset to first page on sort change
    this.fetchProducts(this.searchControl.value); // Re-fetch from backend with new sort
  }

  // --- Product Actions ---

  viewProductDetails(productId: number): void {
    this.router.navigate(['/product-info', productId]);
  }

  editProduct(productId: number): void {
    this.router.navigate(['/edit-product', productId]);
  }

  addProduct(): void {
    this.router.navigate(['/add-product']);
  }

  softDeleteProduct(productId: number): void {
    if (confirm('Are you sure you want to soft delete this product? It can be restored later.')) {
      this.productService.softDeleteProduct(productId).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to soft delete product.';
          return of(null);
        })
      ).subscribe(response => {
        if (response) {
          this.fetchProducts(this.searchControl.value); // Refresh list
        }
      });
    }
  }

  hardDeleteProduct(productId: number): void {
    if (confirm('WARNING: Are you sure you want to PERMANENTLY delete this product? This action cannot be undone.')) {
      this.productService.hardDeleteProduct(productId).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to hard delete product.';
          return of(null);
        })
      ).subscribe(response => {
        if (response) {
          this.fetchProducts(this.searchControl.value); // Refresh list
        }
      });
    }
  }

  // Helper to check if current user is Admin
  isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  // Helper to check if current user is Manager (or Admin)
  isManagerOrAdmin(): boolean {
    return this.authService.isManager() || this.authService.isAdmin();
  }

  onPageChange(page: number) {
    console.log(`Page changed to: ${page}`);
    if (page >= 1 && page <= this.totalPages) {
      this.pageNumber = page;
      this.fetchProducts(this.searchControl.value);
    }
  }
}