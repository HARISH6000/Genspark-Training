import { Component, OnInit } from '@angular/core';
import { NotificationService } from '../services/notification.service';


@Component({
  selector: 'app-notification',
  templateUrl: './notification.html',
  imports: [],
  styleUrls:['./notification.css']
})
export class NotificationComponent implements OnInit {
  username = '';
  message = '';

  constructor(public notifyService: NotificationService) {}

  ngOnInit(): void {
    this.notifyService.startConnection();
  }

  send(): void {
    if (this.username && this.message) {
      this.notifyService.sendMessage(this.username, this.message);
      this.message = '';
    }
  }
}
