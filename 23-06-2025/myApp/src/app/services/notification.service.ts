import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private hubConnection!: signalR.HubConnection;

  public messages: { user: string; message: string }[] = [];

  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:5001/notificationhub')  // 🔁 adjust your backend URL
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.log('SignalR error:', err));

    this.hubConnection.on('ReceiveMessage', (user: string, message: string) => {
      this.messages.push({ user, message });
    });
  }

  sendMessage(user: string, message: string): void {
    this.hubConnection
      .invoke('SendMessage', user, message)
      .catch(err => console.error('Send error:', err));
  }
}
