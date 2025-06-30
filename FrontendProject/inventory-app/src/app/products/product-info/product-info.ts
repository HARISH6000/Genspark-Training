import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Product } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { InventoryService } from '../../services/inventory.service';
import { InventoriesForProduct } from '../../models/inventory';
import { AuthService } from '../../services/auth.service';
import { of, Subscription, combineLatest } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, startWith } from 'rxjs/operators';
import { SortCriterion } from '../../models/sortCriterion';

@Component({
  selector: 'app-product-info',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, CurrencyPipe],
  templateUrl: './product-info.html',
  styleUrl: './product-info.css'
})
export class ProductInfoComponent implements OnInit, OnDestroy {
  productId:number | null = null;
  product: Product | null = null;
  loadingProduct: boolean = true;
  productErrorMessage: string | null = null;

  inventories: InventoriesForProduct[] = [];
  filteredInventories: InventoriesForProduct[] = [];
  loadingInventories: boolean = true;
  inventoryErrorMessage: string | null = null;

  inventorySearchControl = new FormControl('');
  showLowStockOnly: boolean = false;

  private routeSub: Subscription = new Subscription();
  private inventorySearchSub: Subscription = new Subscription();

  
  sortFields = [
    { value: 'inventoryName', name: 'Inventory Name' },
    { value: 'inventoryLocation', name: 'Location' },
    { value: 'quantity', name: 'Available Quantity' },
    { value: 'minstockquantity', name: 'Minimum Stock' }
  ];
  sortOrders = [
    { value: 'asc', name: 'Ascending' },
    { value: 'desc', name: 'Descending' }
  ];
  sortCriteria: SortCriterion[] = [];

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private inventoryService: InventoryService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.routeSub = this.activatedRoute.paramMap.subscribe(params => {
      const productId = Number(params.get('id'));
      this.productId = productId;
      if (productId) {
        this.fetchProductDetails(productId);
        this.fetchInventoriesForProduct(productId);
      } else {
        this.productErrorMessage = 'Product ID not provided.';
        this.loadingProduct = false;
        this.loadingInventories = false;
      }
    });

    this.inventorySearchSub = this.inventorySearchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      startWith('')
    ).subscribe(() => {
      this.fetchInventoriesForProduct(this.productId!);
    });
  }

  ngOnDestroy(): void {
    if (this.routeSub) {
      this.routeSub.unsubscribe();
    }
    if (this.inventorySearchSub) {
      this.inventorySearchSub.unsubscribe();
    }
  }

  fetchProductDetails(productId: number): void {
    console.log("prodId:",productId);
    this.loadingProduct = true;
    this.productErrorMessage = null;
    this.productService.getProductById(productId).pipe(
      catchError(error => {
        this.productErrorMessage = error.message || 'Failed to fetch product details.';
        this.loadingProduct = false;
        return of(null);
      })
    ).subscribe(product => {
      this.product = product;
      this.loadingProduct = false;
    });
  }

  fetchInventoriesForProduct(productId: number): void {
    const searchTerm = this.inventorySearchControl.value?.toLowerCase() || '';
    const orderByParam = this.buildOrderByString();

    this.loadingInventories = true;
    this.inventoryErrorMessage = null;
    this.inventoryService.getInventoriesForProduct(productId, 1, 100, searchTerm,orderByParam).pipe(
      catchError(error => {
        this.inventoryErrorMessage = error.message || 'Failed to fetch inventories for this product.';
        this.loadingInventories = false;
        return of ({data:[],pagination:{totalRecords:0}});
      })
    ).subscribe(res => {
      this.inventories = res.data;
      this.applyInventoryFilters();
      this.loadingInventories = false;
    });
  }

  
  onEditProduct(productId: number): void {
    this.router.navigate(['/edit-product', productId]);
  }

  softDeleteProduct(productId: number): void {
    if (confirm('Are you sure you want to soft delete this product? It can be restored later.')) {
      this.productService.softDeleteProduct(productId).pipe(
        catchError(error => {
          this.productErrorMessage = error.message || 'Failed to soft delete product.';
          return of(null);
        })
      ).subscribe(response => {
        if (response) {
          this.fetchProductDetails(productId);
        }
      });
    }
  }

  hardDeleteProduct(productId: number): void {
    if (confirm('WARNING: Are you sure you want to PERMANENTLY delete this product? This action cannot be undone.')) {
      this.productService.hardDeleteProduct(productId).pipe(
        catchError(error => {
          this.productErrorMessage = error.message || 'Failed to hard delete product.';
          return of(null);
        })
      ).subscribe(response => {
        if (response) {
          this.router.navigate(['/products']);
        }
      });
    }
  }

  // Role Checks
  isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  isManagerOrAdmin(): boolean {
    return this.authService.isManager() || this.authService.isAdmin();
  }

  // Inventory Filtering and Sorting
  applyInventoryFilters(): void {
    let tempInventories = [...this.inventories];
    // Filter by low stock
    if (this.showLowStockOnly) {
      tempInventories = tempInventories.filter(inv => inv.quantityInInventory < inv.minStockQuantity);
    }

    this.filteredInventories = tempInventories;
  }

  onToggleLowStockFilter(): void {
    this.applyInventoryFilters();
  }

  private buildOrderByString(): string {
    return this.sortCriteria
      .filter(c => c.field && c.order)
      .map(c => `${c.field}_${c.order}`)
      .join(',');
  }

  addSortCriterion(): void {
    this.sortCriteria.push({ field: '', order: 'asc' });
  }

  removeSortCriterion(index: number): void {
    this.sortCriteria.splice(index, 1);
    this.onSortCriterionChange(); 
  }

  onSortCriterionChange(): void {
    this.fetchInventoriesForProduct(this.productId!);
  }

  goBack(): void {
    this.router.navigate(['/products']);
  }
}