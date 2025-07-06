import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserDetails } from '../../services/auth.service';
import { InventoryService } from '../../services/inventory.service';
import { Inventory } from '../../models/inventory';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { SortCriterion} from "../../models/sortCriterion";
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-user-info',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-info.html',
  styleUrl: './user-info.css'    
})
export class UserInfoComponent implements OnInit {
  user: UserDetails | null = null;
  inventories: Inventory[] = [];
  filteredInventories: Inventory[] = [];
  loading = true;
  errorMessage = '';

  profilePictureUrl: string = 'https://placehold.co/150x150/EEEEEE/333333?text=User'; // Dummy profile picture
  locationFilter: string = '';

  // Properties for dynamic multi-level sorting
  sortFields = [
    { value: 'name', name: 'Name' },
    { value: 'location', name: 'Location' },
    { value: 'inventoryId', name: 'Inventory ID' }
  ];
  sortOrders = [
    { value: 'asc', name: 'Ascending (A-Z, 0-9)' },
    { value: 'desc', name: 'Descending (Z-A, 9-0)' }
  ];
  sortCriteria: SortCriterion[] = []; // Array to hold multiple sort criteria

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private inventoryService: InventoryService,
    private authService: AuthService,
    private userService: UserService
  ) {
    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras.state && navigation.extras.state['user']) {
      this.user = navigation.extras.state['user'];
      if (this.user?.userId) {
        this.fetchInventories(this.user.userId);
      } else {
        this.loading = false;
        this.errorMessage = 'User ID not found for fetching inventories.';
      }
    } else {
      console.warn('User details not passed via router state. Attempting to get from AuthService.');
      this.user = this.authService.currentUserValue;
      if (this.user?.userId) {
        this.fetchInventories(this.user.userId);
      } else {
        this.loading = false;
        this.errorMessage = 'User ID not found for fetching inventories. Please log in.'; //
        // Optionally redirect to login if no user data even from auth service
        // this.router.navigate(['/login']);
      }
    }
  }

  ngOnInit(): void {
    
    if (this.sortCriteria.length === 0) {
      this.addSortCriterion();
    }
  }

  
  private buildSortByString(): string {
    return this.sortCriteria
      .filter(c => c.field && c.order) 
      .map(c => `${c.field}_${c.order}`)
      .join(',');
  }

  isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  fetchInventories(managerId: number): void {
    this.loading = true;
    this.errorMessage = '';
    const sortByParam = this.buildSortByString(); 
    console.log("srtParam:",sortByParam);

    this.inventoryService.getInventoriesByManagerId(managerId, sortByParam).pipe( 
      catchError(error => {
        console.log("error details:",error);
        this.errorMessage = error.message || 'An error occurred while fetching inventories.';
        if(this.errorMessage.startsWith("Resource not found for")){
          this.errorMessage='';
        }
        this.loading = false;
        return of([]);
      })
    ).subscribe(data => {
      this.inventories = data;
      this.applyLocationFilter();
      this.loading = false;
    });
  }

  
  applyLocationFilter(): void {
    let tempInventories = [...this.inventories];

    if (this.locationFilter) {
      const filterText = this.locationFilter.toLowerCase();
      tempInventories = tempInventories.filter(inv =>
        inv.location.toLowerCase().includes(filterText)
      );
    }
    this.filteredInventories = tempInventories;
  }


  addSortCriterion(): void {
    this.sortCriteria.push({ field: '', order: 'asc' }); 
  }

  removeSortCriterion(index: number): void {
    this.sortCriteria.splice(index, 1);
    this.onSortChange();
  }

  
  onFilterChange(): void {
    this.applyLocationFilter();
  }

  onSortChange(): void {
    
    if (this.user?.userId) {
      this.fetchInventories(this.user.userId);
    }
  }

  editUser(): void {
    if (this.user?.userId) {
      this.router.navigate(['/edit-user', this.user.userId]);
    } else {
      console.error('User ID is not available for editing.');
    }
  }

  deleteUser(): void {
    if (this.user?.userId) {
      if (confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
        this.userService.deleteUser(this.user.userId).subscribe({
          next: () => {
            alert('User deleted successfully.');
            this.router.navigate(['/users']);
          },
          error: err => {
            console.error('Error deleting user:', err);
            alert('Failed to delete user. Please try again later.');
          }
        });
      }
    } else {
      console.error('User ID is not available for deletion.');
    }
  }

  goToInventoryDetails(inventoryId: number): void {
    this.router.navigate(['/inventory', inventoryId]);
  }
}