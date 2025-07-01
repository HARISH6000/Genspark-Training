import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Observable, Subscription, forkJoin, of, combineLatest } from 'rxjs';
import { catchError, map, switchMap, tap, filter as rxFilter } from 'rxjs/operators'; // Renamed filter to rxFilter to avoid conflict
import { FormsModule } from '@angular/forms'; // Import FormsModule for ngModel

// Services
import { AuthService, UserDetails } from '../../services/auth.service';
import { InventoryService } from '../../services/inventory.service';
import { ProductService } from '../../services/product.service';
import { SignalrService } from '../../services/signalr.service';
import { UserService } from '../../services/user.service'; // New service
import { CategoryService } from '../../services/category.service'; // New service

// Models
import { Inventory, Product, ProductsForInventories } from '../../models/inventory';
import { LowStockNotificationDto } from '../../models/notification';
import { PaginationResponse } from '../../models/pagination-response';

// Chart Components
import { InventoryProductPieChartComponent } from '../../shared/inventory-product-pie-chart/inventory-product-pie-chart';
import { InventoryManagersBarChartComponent } from '../../shared/inventory-managers-bar-chart/inventory-managers-bar-chart';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    InventoryProductPieChartComponent,
    InventoryManagersBarChartComponent,
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  currentUser: UserDetails | null = null;
  loading: boolean = true;
  errorMessage: string | null = null;

  // SignalR Notifications
  lowStockNotifications: LowStockNotificationDto[] = [];
  private notificationsSub: Subscription = new Subscription();
  signalRConnectionStatus: string = 'Connecting...';
  private connectionStatusSub: Subscription = new Subscription();

  // Pie Chart Data
  allInventoryProductCounts: { inventoryName: string; totalProducts: number; inventoryId: number; location: string }[] = [];
  filteredInventoryProductCounts: { inventoryName: string; totalProducts: number; inventoryId: number; location: string }[] = [];

  // Bar Chart Data
  allInventoriesWithManagers: { inventoryName: string; managerCount: number; location: string }[] = [];
  filteredInventoriesForBarChart: { inventoryName: string; managerCount: number; location: string }[] = [];


  // Location Filter
  availableLocations: string[] = ['All Locations'];
  pieChartSelectedLocation: string = 'All Locations';
  barChartSelectedLocation: string = 'All Locations';

  // Statistics
  totalProductsCount: number = 0;
  totalCategoriesCount: number = 0;
  totalInventoriesCount: number = 0;
  totalLocationsCount: number = 0;
  totalUsersCount: number = 0;
  totalAdminsCount: number = 0;
  totalManagersCount: number = 0;

  // Recent Items
  recentUsers: UserDetails[] = [];
  recentInventories: Inventory[] = [];
  recentProducts: Product[] = [];

  private subscriptions: Subscription = new Subscription();

  constructor(
    private authService: AuthService,
    private inventoryService: InventoryService,
    private productService: ProductService,
    private signalrService: SignalrService,
    private userService: UserService,
    private categoryService: CategoryService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.currentUser = this.authService.currentUserValue;
    if (!this.currentUser || !this.currentUser.userId) {
      this.errorMessage = 'User not authenticated or user ID not found.';
      this.loading = false;
      this.router.navigate(['/login']); // Redirect if not authenticated
      return;
    }

    this.startSignalRConnection();
    this.fetchDashboardData();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  private startSignalRConnection(): void {
    this.signalrService.startConnection()
      .then(() => {
        console.log('SignalR connection started successfully from user dashboard.');

        // Retrieve stored notifications from sessionStorage
        const storedNotifications = sessionStorage.getItem('lowStockNotifications');
        var allLowStockNotifications = storedNotifications ? JSON.parse(storedNotifications) : [];

        this.lowStockNotifications = allLowStockNotifications.slice(0, 4); 
        this.notificationsSub = this.signalrService.lowStockNotifications$.subscribe(notification => {
          // Add new notification to the beginning of the array
          allLowStockNotifications = [notification, ...allLowStockNotifications];


          if (allLowStockNotifications.length > 4) {
            this.lowStockNotifications = allLowStockNotifications.slice(0, 4);
          }else{
            this.lowStockNotifications = allLowStockNotifications;
          }

          // Save the notifications to sessionStorage
          sessionStorage.setItem('lowStockNotifications', JSON.stringify(allLowStockNotifications));

        });
      })
      .catch(err => {
        console.error('Error starting SignalR connection for user dashboard:', err);
        this.errorMessage = 'Failed to connect to real-time notifications.';
      });

    this.connectionStatusSub = this.signalrService.connectionStatus$.subscribe(isConnected => {
      this.signalRConnectionStatus = isConnected ? 'Connected' : 'Disconnected/Connecting...';
    });
  }

  private fetchDashboardData(): void {
    this.loading = true;
    this.errorMessage = null;

    // Fetch data for statistics, pie chart, bar chart, and recent items concurrently
    forkJoin({
      productsResponse: this.productService.getAllProducts(), // Get all products for total count and recent
      categoriesResponse: this.categoryService.getAllCategories(), // Get all categories for total count
      inventoriesResponse: this.inventoryService.getAllInventories(), // Get all inventories for total count, locations, and recent
      usersResponse: this.userService.getAllUsers(), // Get all users for total count and recent
      admins: this.userService.getAdmins(), // Get admins for count
      managers: this.userService.getManagers(), // Get managers for count
      inventoriesWithManagers: this.inventoryService.getAllInventories(1, 1000) // Re-fetch inventories to get manager counts for bar chart
        .pipe(
          switchMap((inventoriesResponse: PaginationResponse<Inventory>) => {
            const inventories = inventoriesResponse.data;
            const inventoryManagerObservables = inventories.map(inv =>
              this.inventoryService.getManagersByInventoryId(inv.inventoryId)
                .pipe(
                  map(managers => ({
                    inventoryName: inv.name,
                    managerCount: managers.length,
                    location: inv.location
                  })),
                  catchError(err => {
                    console.error(`Error fetching managers for inventory ${inv.inventoryId}:`, err);
                    return of({ inventoryName: inv.name, managerCount: 0, location: inv.location });
                  })
                )
            );
            if (inventoryManagerObservables.length === 0) {
              return of([]);
            }
            return forkJoin(inventoryManagerObservables);
          })
        )
    }).pipe(
      tap(() => this.loading = false),
      catchError(err => {
        this.errorMessage = err.message || 'Failed to load dashboard data.';
        this.loading = false;
        return of({
          productsResponse: { data: [], pagination: null },
          categoriesResponse: [],
          inventoriesResponse: { data: [], pagination: null },
          usersResponse: { data: [], pagination: null },
          admins: [],
          managers: [],
          inventoriesWithManagers: [],
        });
      })
    ).subscribe(results => {

      const { productsResponse,
        categoriesResponse,
        inventoriesResponse,
        usersResponse,
        admins,
        managers,
        inventoriesWithManagers } = results;

      console.log("prres:", productsResponse);
      // Set Statistics
      this.totalProductsCount = productsResponse.data.length;
      this.totalCategoriesCount = categoriesResponse.length;
      this.totalInventoriesCount = inventoriesResponse.data.length;
      this.totalUsersCount = usersResponse.data.length;
      this.totalAdminsCount = admins.length;
      this.totalManagersCount = managers.length;

      // Extract unique locations and set total locations count
      const uniqueLocations = new Set(inventoriesResponse.data.map(inv => inv.location).filter(Boolean));
      this.availableLocations = ['All Locations', ...Array.from(uniqueLocations)];
      this.totalLocationsCount = uniqueLocations.size;

      // Update pie chart data with actual product counts per inventory
      this.inventoryService.getAllInventories(1, 1000).pipe(
        switchMap(allInventoriesResp => {
          const inventoryProductCountObservables = allInventoriesResp.data.map(inv =>
            this.inventoryService.getProductsInInventory(inv.inventoryId, 1, 1000).pipe(
              map(productsResp => ({
                inventoryName: inv.name,
                totalProducts: productsResp.data.reduce((sum, p) => sum + (p.quantityInInventory || 0), 0),
                inventoryId: inv.inventoryId,
                location: inv.location
              })),
              catchError(err => {
                console.error(`Error fetching products for inventory ${inv.inventoryId}:`, err);
                return of({ inventoryName: inv.name, totalProducts: 0, inventoryId: inv.inventoryId, location: inv.location });
              })
            )
          );
          return forkJoin(inventoryProductCountObservables);
        })
      ).subscribe(counts => {
        this.allInventoryProductCounts = counts;
        this.filterChartsByLocation();
      });


      // Prepare Bar Chart Data
      this.allInventoriesWithManagers = inventoriesWithManagers;
      this.filterBarChartsByLocation();


      // Set Recent Items (latest 4)
      this.recentUsers = usersResponse.data.slice(0, 4);
      this.recentInventories = inventoriesResponse.data.slice(0, 4);
      this.recentProducts = productsResponse.data.slice(0, 4);
    });
  }

  onPieChartLocationChange(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    this.pieChartSelectedLocation = selectElement.value;
    this.filterChartsByLocation();
  }

  onBarChartLocationChange(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    this.barChartSelectedLocation = selectElement.value;
    this.filterBarChartsByLocation();
  }

  private filterChartsByLocation(): void {
    if (this.pieChartSelectedLocation === 'All Locations') {
      this.filteredInventoryProductCounts = [...this.allInventoryProductCounts];
    } else {
      this.filteredInventoryProductCounts = this.allInventoryProductCounts.filter(
        item => item.location === this.pieChartSelectedLocation
      );
    }
  }

  private filterBarChartsByLocation(): void {
    if (this.barChartSelectedLocation === 'All Locations') {
      this.filteredInventoriesForBarChart = [...this.allInventoriesWithManagers];
    } else {
      this.filteredInventoriesForBarChart = this.allInventoriesWithManagers.filter(
        item => item.location === this.barChartSelectedLocation
      );
    }
  }

  viewNotifications(): void {
    this.router.navigate(['/notifications']);
  }

  viewAllUsers(): void {
    this.router.navigate(['/admin/users']); // Assuming admin/users is accessible for viewing
  }

  viewAllInventories(): void {
    this.router.navigate(['/inventories']);
  }

  viewAllProducts(): void {
    this.router.navigate(['/products']);
  }

  viewUserInfo(userId: number): void {
    this.router.navigate(['/user-info', userId]);
  }

  viewInventoryDetails(inventoryId: number): void {
    this.router.navigate(['/inventory', inventoryId]);
  }

  viewProductInfo(product: Product): void {
    this.router.navigate(['/product-info', product.productId]);
  }
}
