import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { LowStockNotificationDto } from '../models/notification';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private lowStockNotificationSubject = new BehaviorSubject<LowStockNotificationDto[]>([]); 
  lowStockNotification$ = this.lowStockNotificationSubject.asObservable();

  setLowStockNotification(value: LowStockNotificationDto[]) {
    this.lowStockNotificationSubject.next(value);
  }
}
