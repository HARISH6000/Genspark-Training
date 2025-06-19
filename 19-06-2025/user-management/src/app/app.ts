import { Component } from '@angular/core';
import { RouterOutlet, RouterModule } from '@angular/router';
import { UserFormComponent } from './components/user-form/user-form';
import { UserListComponent } from './components/user-list/user-list';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterModule, UserFormComponent, UserListComponent, CommonModule],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  title = 'angular-user-management';
}