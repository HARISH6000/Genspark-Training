import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-customer-details',
  templateUrl: './customer-details.html',
  styleUrls: ['./customer-details.css'],
  standalone: true,
  imports: [CommonModule] 
})
export class CustomerDetails {
  customerName: string = "John Doe";
  customerId: number = 12345;
  customerEmail: string = "john.doe@example.com";
  likeCount: number = 0;
  dislikeCount: number = 0;

  incrementLike() {
    this.likeCount++;
  }

  incrementDislike() {
    this.dislikeCount++;
  }
}