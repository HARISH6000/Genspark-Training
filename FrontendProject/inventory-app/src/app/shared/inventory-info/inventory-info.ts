import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgSelectModule } from '@ng-select/ng-select';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, combineLatest, of, forkJoin } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, map, startWith, switchMap } from 'rxjs/operators';

import { AuthService, UserDetails } from '../../services/auth.service';
import { InventoryService } from '../../services/inventory.service';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { UserService } from '../../services/user.service'; // Import UserService

import {
  Inventory,
  InventoryProduct,
  CreateInventoryProductRequest,
  QuantityChangeRequest,
  SetQuantityRequest,
  UpdateMinStockRequest,
  Category,
  ProductsForInventories,
  ManagerUser, 
  InventoryManagerAssignmentRequest 
} from '../../models/inventory'; 
import { Product } from '../../models/product'; 
import { User } from '../../models/user'; 
import { PaginationResponse } from '../../models/pagination-response';
import { SortCriterion } from '../../models/sortCriterion';
import { PageNumberComponent } from '../../shared/page-number/page-number';


@Component({
  selector: 'app-inventory-info',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, PageNumberComponent, NgSelectModule],
  templateUrl: './inventory-info.html',
  styleUrls: ['./inventory-info.css']
})
export class InventoryInfoComponent implements OnInit {
  inventory: Inventory | null = null;
  isManagerOfThisInventory: boolean = false; // Renamed to be more specific
  productsInInventory: ProductsForInventories[] = [];
  filteredProductsInInventory: ProductsForInventories[] = [];
  allAvailableProducts: Product[] = []; // For add product dropdown
  availableProductsForDropdown: Product[] = []; // Filtered for dropdown

  showEditInventoryModal: boolean = false;

  // New properties for Inventory Managers section
  assignedManagers: ManagerUser[] = [];
  showAssignManagerForm: boolean = false;
  allUsers: UserDetails[] = []; // All users to populate the assign manager dropdown
  availableUsersForManagerDropdown: UserDetails[] = []; // Filtered users for dropdown
  selectedUserToAssignId: number | null = null;


  categories: Category[] = [];
  selectedCategory: number | null = null;
  showDeleted: boolean = false;

  pageNumber: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  loading: boolean = true;
  errorMessage: string | null = null;

  inventoryId: number | null = null;

  // Filters and Sorts
  searchControl = new FormControl('');
  productSearchControl = new FormControl(''); // For add product dropdown search

  sortCriteria: SortCriterion[] = [{ field: 'productName', order: 'asc' }];
  sortFields = [
    { value: 'productName', name: 'Product Name' },
    { value: 'productSKU', name: 'SKU' },
    { value: 'categoryName', name: 'category' },
    { value: 'quantity', name: 'Quantity' },
    { value: 'minStockQuantity', name: 'Min Stock' }
  ];

  // Add Product Form
  showAddProductForm: boolean = false;
  newProductQuantity: number = 1;
  newProductMinStock: number = 0;
  selectedProductToAddId: number | null = null;

  // Quantity Change for existing products
  actionType: 'add' | 'remove' | 'set' | 'minStock' | null = null;
  activeProductIdForQuantityChange: number | null = null;
  quantityChangeValue: number | null = null;
  minStockChangeValue: number | null = null;

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private inventoryService: InventoryService,
    private productService: ProductService,
    private categoryService: CategoryService,
    private userService: UserService // Inject UserService
  ) { }

  ngOnInit(): void {
    this.inventoryId = Number(this.activatedRoute.snapshot.paramMap.get('id'));

    if (isNaN(this.inventoryId)) {
      this.errorMessage = 'Invalid Inventory ID.';
      this.loading = false;
      return;
    }

    // Initial data fetches that don't depend on inventory details
    this.fetchAllCategories();
    this.fetchAllAvailableProducts();
    this.fetchAllUsersForManagerAssignment(); // Fetch all users for manager assignment

    // CombineLatest for reactive updates based on filters/params
    combineLatest([
      this.searchControl.valueChanges.pipe(startWith(this.searchControl.value)),
      this.activatedRoute.paramMap.pipe(map(params => Number(params.get('id')))),
      this.productSearchControl.valueChanges.pipe(startWith(this.productSearchControl.value), debounceTime(300), distinctUntilChanged()),
      // Include inventory details and assigned managers in the main data stream
      this.inventoryService.getInventoryById(this.inventoryId).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to fetch inventory details.';
          this.loading = false;
          return of(null);
        })
      ),
      this.inventoryService.getManagersByInventoryId(this.inventoryId).pipe( // Fetch assigned managers
        catchError(error => {
          console.error('Failed to fetch assigned managers:', error);
          return of([]);
        })
      )
    ])
      .pipe(
        debounceTime(300), // Debounce to prevent excessive API calls
        distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)), // Only proceed if significant changes
        switchMap(([searchTerm, currentInventoryId, productSearchTerm, inventoryDetails, managers]) => {
          this.loading = true;
          this.errorMessage = null;
          this.inventory = inventoryDetails; // Update inventory details
          this.assignedManagers = managers; // Update assigned managers

          // Determine if current user is a manager of this inventory
          const currentUserId = this.authService.currentUserId??0;
          this.isManagerOfThisInventory = this.authService.isAdmin() ||
            (managers.some(m => m.userId === currentUserId));

          this.filterAvailableProductsForDropdown();
          this.filterAvailableUsersForManagerDropdown(); // Filter users for manager dropdown

          return this.inventoryService.getProductsInInventory(
            currentInventoryId,
            this.pageNumber,
            this.pageSize,
            searchTerm,
            this.buildSortByString()
          ).pipe(
            catchError(error => {
              this.errorMessage = error.message || 'Failed to fetch products in inventory.';
              this.loading = false;
              return of({ data: [], pagination: { totalPages: 0, totalRecords: 0, page: 0, pageSize: 0 } } as PaginationResponse<ProductsForInventories>);
            })
          );
        })
      )
      .subscribe(response => {
        this.productsInInventory = response.data;
        this.totalCount = response.pagination.totalRecords;
        this.totalPages = response.pagination.totalPages;
        this.pageNumber = response.pagination.page; // Ensure current page is updated
        this.filteredProductsInInventory = [...this.productsInInventory]; // No further client-side filter needed as API handles it
        this.loading = false;
      });

    // Initial fetch for products (if not covered by combineLatest's startWith)
    this.fetchProductsInInventory();
  }

  fetchInventoryDetails(id: number): void {
    // This method is now largely covered by the combineLatest in ngOnInit
    // but kept for explicit calls if needed elsewhere.
    this.loading = true;
    this.errorMessage = null;
    this.inventoryService.getInventoryById(id).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to fetch inventory details.';
        this.loading = false;
        return of(null);
      })
    ).subscribe(inventory => {
      this.inventory = inventory;
      this.loading = false;
    });
  }

  fetchProductsInInventory(): void {
    if (this.inventoryId === null) return;
    this.loading = true;
    this.errorMessage = null;
    this.inventoryService.getProductsInInventory(
      this.inventoryId,
      this.pageNumber,
      this.pageSize,
      this.searchControl.value,
      this.buildSortByString()
    ).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to fetch products in inventory.';
        this.loading = false;
        return of({ data: [], pagination: { totalPages: 0, totalRecords: 0, page: 0, pageSize: 0 } } as PaginationResponse<ProductsForInventories>);
      })
    ).subscribe(response => {
      this.productsInInventory = response.data;
      this.totalCount = response.pagination.totalRecords;
      this.totalPages = response.pagination.totalPages;
      this.pageNumber = response.pagination.page;
      this.filteredProductsInInventory = [...this.productsInInventory];
      this.loading = false;
    });
  }

  fetchAllCategories(): void {
    this.categoryService.getAllCategories().pipe(
      catchError(error => {
        console.error('Failed to fetch categories:', error);
        return of([]);
      })
    ).subscribe(categories => {
      this.categories = categories;
    });
  }

  fetchAllAvailableProducts(): void {
    this.productService.getAllProducts(null, null, null, 'productName_asc', false).pipe(
      catchError(error => {
        console.error('Failed to fetch all available products:', error);
        return of({ data: [], pagination: { totalPages: 0, totalRecords: 0, page: 0, pageSize: 0 } } as PaginationResponse<Product>);
      })
    ).subscribe(response => {
      this.allAvailableProducts = response.data;
      this.filterAvailableProductsForDropdown();
    });
  }

  filterAvailableProductsForDropdown(): void {
    const searchTerm = this.productSearchControl.value?.toLowerCase() || '';
    this.availableProductsForDropdown = this.allAvailableProducts.filter(product =>
      !this.productsInInventory.some(ip => ip.productId === product.productId) &&
      (product.productName.toLowerCase().includes(searchTerm) || product.sku.toLowerCase().includes(searchTerm))
    );
  }

  customProductSearchFn = (term: string, item: any) => {
    term = term.toLowerCase();
    return (
      item.productName.toLowerCase().includes(term) ||
      item.sku.toLowerCase().includes(term)
    );
  };

  
  fetchAssignedManagers(): void {
    if (this.inventoryId === null) return;
    this.inventoryService.getManagersByInventoryId(this.inventoryId).pipe(
      catchError(error => {
        console.error('Failed to fetch assigned managers:', error);
        return of([]);
      })
    ).subscribe(managers => {
      this.assignedManagers = managers;
      this.filterAvailableUsersForManagerDropdown(); 
    });
  }

  fetchAllUsersForManagerAssignment(): void {
    
    this.userService.getAllUsers().pipe( 
      catchError(error => {
        console.error('Failed to fetch all users for manager assignment:', error);
        return of({ data: [], pagination:{ totalPages: 0, totalRecords: 0, page: 0, pageSize: 0 } } as PaginationResponse<User>);
      })
    ).subscribe(response => {
      this.allUsers = response.data;
      this.filterAvailableUsersForManagerDropdown();
    });
  }

  filterAvailableUsersForManagerDropdown(): void {
    if (!this.allUsers || !this.assignedManagers) {
      this.availableUsersForManagerDropdown = [];
      return;
    }
    // Filter out users who are already assigned as managers to this inventory
    this.availableUsersForManagerDropdown = this.allUsers.filter(user =>
      !this.assignedManagers.some(manager => manager.userId === user.userId)
    );
  }

  customUserSearchFn = (term: string, item: any) => {
    term = term.toLowerCase();
    return item.username.toLowerCase().includes(term)||item.email.toLowerCase().includes(term) || item.userId.toString().includes(term);
  };

  toggleAssignManagerForm(): void {
    this.showAssignManagerForm = !this.showAssignManagerForm;
    this.selectedUserToAssignId = null; // Reset selected user
    if (this.showAssignManagerForm) {
      this.filterAvailableUsersForManagerDropdown(); // Refresh dropdown when opening
    }
  }

  assignManagerToInventory(): void {
    if (this.inventoryId === null || this.selectedUserToAssignId === null) {
      this.errorMessage = 'Please select a user to assign.';
      return;
    }

    const request: InventoryManagerAssignmentRequest = {
      inventoryId: this.inventoryId,
      managerId: this.selectedUserToAssignId
    };

    this.inventoryService.assignInventoryToManager(request).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to assign manager.';
        return of(null);
      })
    ).subscribe(response => {
      if (response) {
        alert('Manager assigned successfully!');
        this.showAssignManagerForm = false;
        this.fetchAssignedManagers(); // Refresh assigned managers list
        this.filterAvailableUsersForManagerDropdown(); // Update available users
      }
    });
  }

  removeManagerFromInventory(userId: number): void {
    if (this.inventoryId === null) return;

    if (confirm('Are you sure you want to remove this manager from the inventory?')) {
      const request: InventoryManagerAssignmentRequest = {
        inventoryId: this.inventoryId,
        managerId: userId
      };

      this.inventoryService.removeInventoryFromManager(request).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to remove manager.';
          return of(null);
        })
      ).subscribe(response => {
        if (response) {
          alert('Manager removed successfully!');
          this.fetchAssignedManagers(); // Refresh assigned managers list
          this.filterAvailableUsersForManagerDropdown(); // Update available users
        }
      });
    }
  }
  


  onCategoryChange(): void {
    this.pageNumber = 1;
    this.fetchProductsInInventory();
  }

  onShowDeletedChange(): void {
    this.pageNumber = 1;
    this.fetchProductsInInventory();
  }

  onPageChange(page: number): void {
    console.log(`Page changed to: ${page}`);
    if (page >= 1 && page <= this.totalPages) {
      this.pageNumber = page;
      this.fetchProductsInInventory();
    }
  }

  onPageSizeChange(): void {
    this.pageNumber = 1; // Reset to first page when page size changes
    this.fetchProductsInInventory();
  }

  addSortCriterion(): void {
    this.sortCriteria.push({ field: '', order: 'asc' });
  }

  removeSortCriterion(index: number): void {
    this.sortCriteria.splice(index, 1);
    this.fetchProductsInInventory();
  }

  onSortChange(): void {
    this.fetchProductsInInventory();
  }

  private buildSortByString(): string {
    return this.sortCriteria
      .filter(c => c.field && c.order)
      .map(c => `${c.field}_${c.order}`)
      .join(',');
  }

  // Admin Actions
  editInventory(): void {
    if (this.inventory) {
      this.showEditInventoryModal = true;
    }
  }

  deleteInventory(): void {
    if (this.inventory && confirm('Are you sure you want to delete this inventory?')) {
      this.inventoryService.softDeleteInventory(this.inventory.inventoryId).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to delete inventory.';
          return of(null);
        })
      ).subscribe(response => {
        if (response) {
          alert('Inventory deleted successfully!');
          this.router.navigate(['/inventories']);
        }
      });
    }
  }

  // Manager/Admin Actions for products within inventory
  openAddProductForm(): void {
    this.showAddProductForm = true;
    this.selectedProductToAddId = null;
    this.newProductQuantity = 1;
    this.newProductMinStock = 0;
    this.filterAvailableProductsForDropdown(); // Re-filter when opening the form
  }

  addProductToInventory(): void {
    if (this.inventoryId === null || this.selectedProductToAddId === null || this.newProductQuantity <= 0) {
      this.errorMessage = 'Please select a product and enter a valid quantity.';
      return;
    }

    const request: CreateInventoryProductRequest = {
      inventoryId: this.inventoryId,
      productId: this.selectedProductToAddId,
      quantity: this.newProductQuantity,
      minStockQuantity: this.newProductMinStock
    };

    this.inventoryService.createInventoryProduct(request).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to add product to inventory.';
        return of(null);
      })
    ).subscribe(response => {
      if (response) {
        alert('Product added to inventory successfully!');
        this.showAddProductForm = false;
        this.fetchProductsInInventory(); // Refresh list
        this.fetchAllAvailableProducts(); // Refresh available products for dropdown
      }
    });
  }

  cancelAddProduct(): void {
    this.showAddProductForm = false;
    this.errorMessage = null;
  }

  openQuantityChangeInput(productId: number, type: 'add' | 'remove' | 'set' | 'minStock', currentQuantity?: number, currentMinStock?: number): void {
    this.activeProductIdForQuantityChange = productId;
    this.actionType = type;
    this.errorMessage = null; // Clear previous errors

    if (type === 'add' || type === 'remove' || type === 'set') {
      this.quantityChangeValue = null;
    } else if (type === 'minStock') {
      this.minStockChangeValue = null;
    }
  }

  submitQuantityChange(): void {
    if (this.inventoryId === null || this.activeProductIdForQuantityChange === null) {
      this.errorMessage = 'Invalid operation. Missing inventory or product ID.';
      return;
    }

    if (this.actionType === 'add' || this.actionType === 'remove') {
      if (this.quantityChangeValue === null || isNaN(this.quantityChangeValue) || this.quantityChangeValue <= 0) {
        this.errorMessage = 'Please enter a valid quantity.';
        return;
      }
    } else if (this.actionType === 'set') {
      if (this.quantityChangeValue === null || isNaN(this.quantityChangeValue) || this.quantityChangeValue < 0) {
        this.errorMessage = 'Please enter a valid quantity (non-negative).';
        return;
      }
    } else if (this.actionType === 'minStock') {
      if (this.minStockChangeValue === null || isNaN(this.minStockChangeValue) || this.minStockChangeValue < 0) {
        this.errorMessage = 'Please enter a valid minimum stock quantity (non-negative).';
        return;
      }
    }

    let observable: Observable<any> | null = null;

    switch (this.actionType) {
      case 'add':
        observable = this.inventoryService.increaseInventoryProductQuantity({
          inventoryId: this.inventoryId,
          productId: this.activeProductIdForQuantityChange,
          quantityChange: this.quantityChangeValue!
        });
        break;
      case 'remove':
        observable = this.inventoryService.decreaseInventoryProductQuantity({
          inventoryId: this.inventoryId,
          productId: this.activeProductIdForQuantityChange,
          quantityChange: this.quantityChangeValue!
        });
        break;
      case 'set':
        observable = this.inventoryService.setInventoryProductQuantity({
          inventoryId: this.inventoryId,
          productId: this.activeProductIdForQuantityChange,
          newQuantity: this.quantityChangeValue!
        });
        break;
      case 'minStock':
        observable = this.inventoryService.updateInventoryProductMinStock({
          inventoryId: this.inventoryId,
          productId: this.activeProductIdForQuantityChange,
          newMinStockQuantity: this.minStockChangeValue!
        });
        break;
      default:
        this.errorMessage = 'Invalid action type.';
        return;
    }

    observable.pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to update product quantity/min stock.';
        return of(null);
      })
    ).subscribe(response => {
      if (response) {
        this.fetchProductsInInventory(); // Refresh the list
        this.cancelQuantityChange(); // Reset input state
      }
    });
  }

  cancelQuantityChange(): void {
    this.activeProductIdForQuantityChange = null;
    this.actionType = null;
    this.quantityChangeValue = null;
    this.minStockChangeValue = null;
    this.errorMessage = null; 
  }

  removeProductFromInventory(inventoryProductId: number): void {
    if (confirm('Are you sure you want to remove this product from the inventory? This action cannot be undone.')) {
      this.inventoryService.deleteInventoryProduct(inventoryProductId).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to remove product from inventory.';
          return of(null);
        })
      ).subscribe(response => {
        if (response) {
          this.fetchProductsInInventory(); 
          this.fetchAllAvailableProducts();
        }
      });
    }
  }

  isNaN(value: any): boolean {
    return isNaN(value);
  }

  toggleAddProductForm(): void {
    this.showAddProductForm = !this.showAddProductForm;
  }

  toggleEditInventoryModal(): void {
    this.showEditInventoryModal = !this.showEditInventoryModal;
  }

  saveInventory(): void {
    if (!this.inventory) return;
    this.inventoryService.updateInventory(this.inventory).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to update inventory.';
        return of(null);
      })
    ).subscribe(response => {
      if (response) {
        alert('Inventory updated successfully!');
        this.toggleEditInventoryModal();
      }
    });
  }


  // Role Checkers
  isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  isManagerOrAdmin(): boolean {
    return this.authService.isManager() || this.authService.isAdmin();
  }

  goBack(): void {
    this.router.navigate(['/inventories']); 
  }
}
