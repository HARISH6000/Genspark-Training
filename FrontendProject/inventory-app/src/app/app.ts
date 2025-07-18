import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './shared/navbar/navbar';
import { SignalrService } from './services/signalr.service';
import { NotificationService } from './services/notification.service';
import { LowStockNotificationDto } from './models/notification';
import { catchError, Subscription } from 'rxjs';
import { AuthService } from './services/auth.service';
import { InventoryService } from './services/inventory.service';
import { Inventory } from './models/inventory';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, NavbarComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit, OnDestroy {
  title = 'inventory-app';

  notifications: LowStockNotificationDto[] = [];
  private notificationsSub: Subscription = new Subscription();
  private connectionStatusSub: Subscription = new Subscription();

  constructor(private signalrService: SignalrService, private notificationService: NotificationService, private authService: AuthService, private inventoryService: InventoryService) { }

  ngOnInit() {

    this.signalrService.startConnection().catch(err => {
      console.error('Error starting SignalR connection:', err);
    });

    let storedNotifications = sessionStorage.getItem('lowStockNotifications');
    this.notifications = storedNotifications ? JSON.parse(storedNotifications) : [];

    this.notificationService.setLowStockNotification(this.notifications);

    this.notificationsSub = this.signalrService.lowStockNotifications$.subscribe(notification => {

      storedNotifications = sessionStorage.getItem('lowStockNotifications');
      this.notifications = storedNotifications ? JSON.parse(storedNotifications) : [];

      const isAdmin = this.authService.isAdmin();

      if (notification.sku) {
        if (!isAdmin) {

          this.inventoryService.getInventoriesByManagerId(this.authService.currentUserId ?? 0).pipe(catchError(error => {
            return [];
          })).subscribe(inventories => {
            if (inventories.some(inventory => inventory.inventoryId === notification.inventoryId)) {
              this.notifications = [notification, ...this.notifications];
              this.notificationService.setLowStockNotification(this.notifications);
              sessionStorage.setItem('lowStockNotifications', JSON.stringify(this.notifications));
            }
          });
        }
        else {
          this.notifications = [notification, ...this.notifications];
          this.notificationService.setLowStockNotification(this.notifications);
          sessionStorage.setItem('lowStockNotifications', JSON.stringify(this.notifications));
        }
      }
      else{
        if(isAdmin) {
          this.notifications = [notification, ...this.notifications];
          this.notificationService.setLowStockNotification(this.notifications);
          sessionStorage.setItem('lowStockNotifications', JSON.stringify(this.notifications));
        }
        else if (notification.productId === this.authService.currentUserId) {
          this.notifications = this.notifications.filter(n => n.productId === this.authService.currentUserId || n.sku);
          this.notifications = [notification, ...this.notifications];
          this.notificationService.setLowStockNotification(this.notifications);
          sessionStorage.setItem('lowStockNotifications', JSON.stringify(this.notifications));
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.notificationsSub.unsubscribe();
    this.connectionStatusSub.unsubscribe();
  }
}