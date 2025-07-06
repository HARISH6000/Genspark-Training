import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { Observable, combineLatest, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, startWith, map } from 'rxjs/operators';
import { Inventory, CreateInventoryRequest, UpdateInventoryRequest } from '../../models/inventory';
import { PaginationResponse } from '../../models/pagination-response';
import { PageNumberComponent } from '../../shared/page-number/page-number';
import { InventoryService } from '../../services/inventory.service';
import { AuthService } from '../../services/auth.service';
import { SortCriterion } from '../../models/sortCriterion';
import { Router, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-inventory-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, PageNumberComponent],
  templateUrl: './inventory-management.html',
  styleUrls: ['./inventory-management.css']
})
export class InventoryManagementComponent implements OnInit {
  inventories: Inventory[] = [];
  loading: boolean = true;
  errorMessage: string | null = null;
  includeDeleted:boolean =false;

  // Pagination properties
  pageNumber: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  totalPages: number = 0;

  // Search and Sort
  searchControl = new FormControl('', { nonNullable: true });
  sortCriteria: SortCriterion[] = [
    { field: 'name', order: 'asc' } // Default sort
  ];
  sortFields = [
    { value: 'name', name: 'Inventory Name' },
    { value: 'location', name: 'Location' },
    { value: 'inventoryId', name: 'Inventory ID' }
  ];

  // Add/Edit Form properties
  showForm: boolean = false;
  isEditMode: boolean = false;
  currentInventory: Inventory | null = null;
  newInventory: CreateInventoryRequest = { name: '', location: '' };

  constructor(
    private inventoryService: InventoryService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    combineLatest([
      this.searchControl.valueChanges.pipe(
        startWith(this.searchControl.value),
        debounceTime(300),
        distinctUntilChanged()
      )
    ]).subscribe(([searchTerm]) => {
      this.pageNumber = 1; // Reset page to 1 on search or filter change
      this.fetchInventories(searchTerm);
    });

    this.fetchInventories(this.searchControl.value);
  }

  fetchInventories(searchTerm: string | null = null): void {
    this.loading = true;
    this.errorMessage = null;

    const orderBy = this.buildSortByString();

    this.inventoryService.getAllInventories(
      this.pageNumber,
      this.pageSize,
      searchTerm,
      orderBy,
      this.includeDeleted
    ).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to fetch inventories.';
        this.loading = false;
        return of({ data: [], pagination: { totalRecords: 0, page: 0, pageSize: 0, totalPages: 0 } } as PaginationResponse<Inventory>);
      })
    ).subscribe(response => {
      this.inventories = response.data;
      this.totalCount = response.pagination.totalRecords;
      this.totalPages = response.pagination.totalPages;
      this.loading = false;
    });
  }

  private buildSortByString(): string {
    return this.sortCriteria
      .filter(c => c.field && c.order)
      .map(c => `${c.field}_${c.order}`)
      .join(',');
  }

  onSortChange(): void {
    this.pageNumber = 1; // Reset page to 1 on sort change
    this.fetchInventories(this.searchControl.value);
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.pageNumber = page;
      this.fetchInventories(this.searchControl.value);
    }
  }

  onPageSizeChange(): void {
    this.pageNumber = 1; // Reset page to 1 when page size changes
    this.fetchInventories(this.searchControl.value);
  }

  openAddForm(): void {
    this.isEditMode = false;
    this.currentInventory = null;
    this.newInventory = { name: '', location: '' };
    this.showForm = true;
    this.errorMessage = null; // Clear previous errors
  }

  openEditForm(inventory: Inventory): void {
    this.isEditMode = true;
    this.currentInventory = { ...inventory }; // Create a copy to avoid direct mutation
    this.newInventory = { name: inventory.name, location: inventory.location };
    this.showForm = true;
    this.errorMessage = null; // Clear previous errors
  }

  closeForm(): void {
    this.showForm = false;
    this.currentInventory = null;
    this.newInventory = { name: '', location: '' };
    this.errorMessage = null;
  }

  isFormValid(): boolean {
    return this.newInventory.name.trim().length >= 3 &&
           this.newInventory.name.trim().length <= 100 &&
           this.newInventory.location.trim().length >= 3 &&
           this.newInventory.location.trim().length <= 200;
  }

  submitForm(): void {
    if (this.isEditMode) {
      this.updateInventory();
    } else {
      this.addInventory();
    }
  }

  addInventory(): void {
    this.errorMessage = null;
    this.inventoryService.createInventory(this.newInventory).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to add inventory.';
        return of(null);
      })
    ).subscribe(response => {
      if (response) {
        this.fetchInventories(this.searchControl.value); // Refresh the list
        this.closeForm();
      }
    });
  }

  updateInventory(): void {
    if (!this.currentInventory) return;

    const updateRequest: UpdateInventoryRequest = {
      inventoryId: this.currentInventory.inventoryId,
      name: this.newInventory.name,
      location: this.newInventory.location
    };

    this.errorMessage = null;
    this.inventoryService.updateInventory(updateRequest).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to update inventory.';
        return of(null);
      })
    ).subscribe(response => {
      if (response) {
        this.fetchInventories(this.searchControl.value);
        this.closeForm();
      }
    });
  }

  softDeleteInventory(inventoryId: number): void {
    if (confirm('Are you sure you want to soft delete this inventory? It can be restored later.')) {
      this.errorMessage = null;
      this.inventoryService.softDeleteInventory(inventoryId).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to soft delete inventory.';
          return of(null);
        })
      ).subscribe(response => {
        if (response) {
          this.fetchInventories(this.searchControl.value); // Refresh list
        }
      });
    }
  }

  hardDeleteInventory(inventoryId: number): void {
    if (confirm('WARNING: Are you sure you want to PERMANENTLY delete this inventory? This action cannot be undone.')) {
      this.errorMessage = null;
      this.inventoryService.hardDeleteInventory(inventoryId).pipe(
        catchError(error => {
          this.errorMessage = error.message || 'Failed to hard delete inventory.';
          return of(null);
        })
      ).subscribe(response => {
        if (response) {
          this.fetchInventories(this.searchControl.value); // Refresh list
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

  // Sort criteria management
  addSortCriterion(): void {
    this.sortCriteria.push({ field: '', order: 'asc' });
  }

  removeSortCriterion(index: number): void {
    this.sortCriteria.splice(index, 1);
    this.onSortChange(); // Re-apply sort after removal
  }

  viewInventoryDetails(inventory: Inventory): void {
    this.router.navigate(['/inventory', inventory.inventoryId]);
  }
}