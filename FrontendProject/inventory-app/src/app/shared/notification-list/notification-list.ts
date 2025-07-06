import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SignalrService } from '../../services/signalr.service';
import { AuthService } from '../../services/auth.service';
import { LowStockNotificationDto } from '../../models/notification';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router'; 
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-list.html',
  styleUrls: ['./notification-list.css']
})
export class NotificationListComponent implements OnInit {
  notifications: LowStockNotificationDto[] = [];
  signalRConnectionStatus: string = 'Connecting...';
  private connectionStatusSub: Subscription = new Subscription();
  

  isManger: boolean = false; 

  constructor(private signalrService: SignalrService,private notificationService: NotificationService, private authService: AuthService,private router: Router) { }

  ngOnInit(): void {

    this.isManger = this.authService.isManager();

    this.notificationService.lowStockNotification$.subscribe((notifications: LowStockNotificationDto[]) => {
        this.notifications = notifications;
    });

    this.connectionStatusSub = this.signalrService.connectionStatus$.subscribe(isConnected => {
      this.signalRConnectionStatus = isConnected ? 'Connected' : 'Disconnected/Connecting...';
    });
    
  }

  

  
  clearAllNotifications(): void {
    if (confirm('Are you sure you want to clear all notifications? This action cannot be undone.')) {
      this.notifications = [];
      this.notificationService.setLowStockNotification(this.notifications);
      sessionStorage.removeItem('lowStockNotifications'); 
    }
  }

  
  clearNotification(index: number): void {
    if (index > -1 && index < this.notifications.length) {
      this.notifications.splice(index, 1); 
      this.notificationService.setLowStockNotification(this.notifications);
      sessionStorage.setItem('lowStockNotifications', JSON.stringify(this.notifications)); 
    }
  }

  
  goBack(): void {
    this.router.navigate(['/']);
  }
}
