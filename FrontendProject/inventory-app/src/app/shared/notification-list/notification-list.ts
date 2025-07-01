import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SignalrService } from '../../services/signalr.service';
import { LowStockNotificationDto } from '../../models/notification';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router'; // Import Router for navigation

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-list.html',
  styleUrls: ['./notification-list.css']
})
export class NotificationListComponent implements OnInit, OnDestroy {
  notifications: LowStockNotificationDto[] = [];
  signalRConnectionStatus: string = 'Connecting...';
  private notificationsSub: Subscription = new Subscription();
  private connectionStatusSub: Subscription = new Subscription();

  constructor(private signalrService: SignalrService, private router: Router) { }

  ngOnInit(): void {
    // Ensure SignalR connection is started (it should ideally be started globally in AppComponent)
    // This is a fallback to ensure it's active if navigating directly.
    this.signalrService.startConnection().catch(err => {
      console.error('Error starting SignalR connection:', err);
      // Optionally, set an error message for the user
    });

    // Load notifications from sessionStorage on init
    const storedNotifications = sessionStorage.getItem('lowStockNotifications');
    this.notifications = storedNotifications ? JSON.parse(storedNotifications) : [];

    // Subscribe to new notifications
    this.notificationsSub = this.signalrService.lowStockNotifications$.subscribe(notification => {
      // Add new notification to the beginning of the array
      this.notifications = [notification, ...this.notifications];
      // Update sessionStorage with the new list
      sessionStorage.setItem('lowStockNotifications', JSON.stringify(this.notifications));
    });

    // Subscribe to connection status
    this.connectionStatusSub = this.signalrService.connectionStatus$.subscribe(isConnected => {
      this.signalRConnectionStatus = isConnected ? 'Connected' : 'Disconnected/Connecting...';
    });
  }

  ngOnDestroy(): void {
    this.notificationsSub.unsubscribe();
    this.connectionStatusSub.unsubscribe();
    // Do NOT stop SignalR connection here if it's meant to be app-wide.
    // The SignalrService itself has an ngOnDestroy that handles stopping.
  }

  /**
   * Clears all notifications from the list and session storage.
   */
  clearAllNotifications(): void {
    if (confirm('Are you sure you want to clear all notifications? This action cannot be undone.')) {
      this.notifications = [];
      sessionStorage.removeItem('lowStockNotifications'); // Clear from sessionStorage
    }
  }

  /**
   * Clears a single notification by its index from the list and session storage.
   * @param index The index of the notification to clear.
   */
  clearNotification(index: number): void {
    if (index > -1 && index < this.notifications.length) {
      this.notifications.splice(index, 1); // Remove the notification
      sessionStorage.setItem('lowStockNotifications', JSON.stringify(this.notifications)); // Update sessionStorage
    }
  }

  /**
   * Navigates back to the main dashboard.
   */
  goBack(): void {
    this.router.navigate(['/']); // Navigate back to the root/dashboard
  }
}
