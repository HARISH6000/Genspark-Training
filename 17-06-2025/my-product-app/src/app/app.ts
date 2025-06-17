import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router'; 
import { AuthService } from './auth'; 
import { NgIf } from '@angular/common'; 

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgIf], 
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent {
  title = 'Angular Product Browser';

  constructor(public authService: AuthService, private router: Router) { }

  isLoggedIn(): boolean {
    return this.authService.isAuthenticated();
  }

  
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']); 
  }
}
