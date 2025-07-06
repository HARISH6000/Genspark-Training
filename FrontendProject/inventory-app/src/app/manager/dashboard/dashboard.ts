import { Component, OnInit, OnDestroy, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Observable, Subscription, forkJoin, of, combineLatest } from 'rxjs';
import { catchError, map, switchMap, filter, tap } from 'rxjs/operators';
import { HubConnectionState } from '@microsoft/signalr';

// Services
import { AuthService, UserDetails } from '../../services/auth.service';
import { InventoryService } from '../../services/inventory.service';
import { ProductService } from '../../services/product.service';
import { SignalrService } from '../../services/signalr.service';

// Models
import { Inventory, Product, InventoryManager, ProductsForInventories } from '../../models/inventory';
import { LowStockNotificationDto } from '../../models/notification';
import { PaginationResponse } from '../../models/pagination-response';

// Chart Component
import { InventoryProductPieChartComponent } from '../../shared/inventory-product-pie-chart/inventory-product-pie-chart';
import { NotificationService } from '../../services/notification.service';

// D3.js is no longer directly used for charting in this component, but can remain if used elsewhere
// import * as d3 from 'd3';


@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule, InventoryProductPieChartComponent], // Add new chart component here
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class ManagerDashboardComponent implements OnInit, OnDestroy, AfterViewInit {
  // @ViewChild('pieChartContainer', { static: false }) pieChartContainer!: ElementRef; // No longer needed

  currentUser: UserDetails | null = null;
  loading: boolean = true;
  errorMessage: string | null = null;

  // SignalR Notifications
  lowStockNotifications: LowStockNotificationDto[] = [];
  private notificationsSub: Subscription = new Subscription();
  signalRConnectionStatus: string = 'Connecting...';
  private connectionStatusSub: Subscription = new Subscription();


  // Inventory Data
  managedInventories: Inventory[] = [];
  inventoryProductCounts: { inventoryName: string; totalProducts: number; inventoryId: number }[] = [];

  // Featured Products (random selection)
  featuredProducts: Product[] = [];

  private subscriptions: Subscription = new Subscription();

  constructor(
    private authService: AuthService,
    private inventoryService: InventoryService,
    private productService: ProductService,
    private signalrService: SignalrService,
    private notificationService: NotificationService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.currentUser = this.authService.currentUserValue;
    if (!this.currentUser || !this.currentUser.userId) {
      this.errorMessage = 'User not authenticated or user ID not found.';
      this.loading = false;
      this.router.navigate(['/login']); 
      return;
    }

    this.notificationService.lowStockNotification$.subscribe((notifications: LowStockNotificationDto[]) => {
      if(notifications.length > 4) {
        this.lowStockNotifications = notifications.slice(0, 4);
      }
      else{
        this.lowStockNotifications = notifications;
      }
    });

    this.connectionStatusSub = this.signalrService.connectionStatus$.subscribe(isConnected => {
      this.signalRConnectionStatus = isConnected ? 'Connected' : 'Disconnected/Connecting...';
    });
    
    this.fetchDashboardData();
  }

  ngAfterViewInit(): void {
    
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  private fetchDashboardData(): void {
    this.loading = true;
    this.errorMessage = null;

    const managerId = this.currentUser!.userId;

    forkJoin([
      this.inventoryService.getInventoriesByManagerId(managerId),
      this.productService.getAllProducts(1, 4, null, null, false)
    ]).pipe(
      switchMap(([inventories, featuredProductsResponse]) => {
        this.managedInventories = inventories;
        this.featuredProducts = featuredProductsResponse.data;

        // For pie chart, fetch product count for each managed inventory
        const inventoryProductCountObservables = inventories.map(inv =>
          this.inventoryService.getProductsInInventory(inv.inventoryId, 1, 1000) // Fetch all products in inventory for counting
            .pipe(
              map(resp => ({
                inventoryName: inv.name,
                // Assuming resp.data is an array of Product objects, and Product has a 'quantity' property.
                // If ProductsForInventories is structured differently, adjust this line.
                totalProducts: resp.data.reduce((sum, p) => sum + (p.quantityInInventory || 0), 0), // Sum quantities of products
                inventoryId: inv.inventoryId
              })),
              catchError(err => {
                console.error(`Error fetching products for inventory ${inv.inventoryId}:`, err);
                return of({ inventoryName: inv.name, totalProducts: 0, inventoryId: inv.inventoryId }); // Return 0 if error
              })
            )
        );

        // If no inventories managed, return empty array for counts immediately
        if (inventoryProductCountObservables.length === 0) {
            return of({ counts: [], inventories, featuredProducts: featuredProductsResponse.data });
        }

        return forkJoin(inventoryProductCountObservables).pipe(
          map(counts => ({ counts, inventories, featuredProducts: featuredProductsResponse.data }))
        );
      }),
      tap(() => this.loading = false), // Set loading to false after forkJoin completes
      catchError(err => {
        this.errorMessage = err.message || 'Failed to load dashboard data.';
        this.loading = false;
        return of({ counts: [], inventories: [], featuredProducts: [] });
      })
    ).subscribe(({ counts, inventories, featuredProducts }) => {
      this.inventoryProductCounts = counts;
      this.managedInventories = inventories;
      this.featuredProducts = featuredProducts;
      // The chart component will update itself via ngOnChanges
    });
  }

  // renderPieChart(): void { /* Removed, handled by child component */ }

  viewNotifications(): void {
    this.router.navigate(['/notifications']);
  }

  viewAllProducts(): void {
    this.router.navigate(['/products']);
  }

  viewInventoryDetails(inventoryId: number): void {
    this.router.navigate(['/inventory', inventoryId]);
  }

  // Role Checkers
  isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  isManager(): boolean {
    return this.authService.isManager();
  }

  // Helper method for safely getting user ID
  get currentUserId(): number | null {
    return this.currentUser?.userId || null;
  }

  isNoProducts(): boolean {
    return (
      this.inventoryProductCounts.length === 0 ||
      this.inventoryProductCounts.every(d => d.totalProducts === 0)
    );
  }

  viewProductInfo(product:Product){
    this.router.navigate(['/product-info', product.productId]);
  }

}
