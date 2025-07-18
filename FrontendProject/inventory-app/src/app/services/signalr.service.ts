import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr'; 
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { LowStockNotificationDto} from '../models/notification'; 
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignalrService implements OnDestroy {
  private hubConnection: signalR.HubConnection | undefined;
  private lowStockNotificationSubject = new Subject<LowStockNotificationDto>();
  public lowStockNotifications$ = this.lowStockNotificationSubject.asObservable();

  // Keep track of connection status
  private connectionStatusSubject = new BehaviorSubject<boolean>(false);
  public connectionStatus$ = this.connectionStatusSubject.asObservable();

  constructor() { }

  /**
   * Starts the SignalR connection.
   * Connects to the /lowstock-notifications hub.
   */
  public startConnection(): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR connection already established.');
      return Promise.resolve();
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/lowstock-notifications`) // Use environment variable
      .withAutomaticReconnect()
      .build();

    this.hubConnection.onclose(error => {
      console.error('SignalR connection closed:', error);
      this.connectionStatusSubject.next(false);
      // Optional: implement custom reconnect logic here if .withAutomaticReconnect() isn't enough
    });

    this.hubConnection.onreconnecting(error => {
      console.warn('SignalR reconnecting...', error);
      this.connectionStatusSubject.next(false);
    });

    this.hubConnection.onreconnected(connectionId => {
      console.log('SignalR reconnected. Connection ID:', connectionId);
      this.connectionStatusSubject.next(true);
    });

    this.hubConnection.on('ReceiveLowStockNotification', (notification: LowStockNotificationDto) => {
      console.log('Received Low Stock Notification:', notification);
      this.lowStockNotificationSubject.next(notification);
    });

    this.hubConnection.on('ReceiveManagerChangeNotification', (notification: LowStockNotificationDto) => {
      console.log('Received Manager Change Notification:', notification);
      this.lowStockNotificationSubject.next(notification);
    });

    return this.hubConnection.start()
      .then(() => {
        console.log('SignalR connection started.');
        this.connectionStatusSubject.next(true);
      })
      .catch(err => {
        console.error('Error while starting SignalR connection: ' + err);
        this.connectionStatusSubject.next(false);
        throw err; // Re-throw to propagate the error
      });
  }

  /**
   * Stops the SignalR connection.
   */
  public stopConnection(): Promise<void> {
    if (this.hubConnection) {
      return this.hubConnection.stop()
        .then(() => {
          console.log('SignalR connection stopped.');
          this.connectionStatusSubject.next(false);
        })
        .catch(err => console.error('Error while stopping SignalR connection: ' + err));
    }
    return Promise.resolve();
  }

  ngOnDestroy(): void {
    this.stopConnection();
    this.lowStockNotificationSubject.complete();
    this.connectionStatusSubject.complete();
  }
}