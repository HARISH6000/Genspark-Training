import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router'; 

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [CommonModule, RouterLink], 
  templateUrl: './landing-page.html',
  styleUrls: ['./landing-page.css']
})
export class LandingPageComponent implements OnInit {
  currentYear: number; 

  constructor() {
    this.currentYear = new Date().getFullYear(); 
  }

  ngOnInit(): void {
   
  }
}
