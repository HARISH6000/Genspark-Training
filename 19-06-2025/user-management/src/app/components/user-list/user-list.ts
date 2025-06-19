import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';
import { User } from '../../models/user.model';
import { UserService } from '../../services/user.service';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-list.html',
  styleUrls: ['./user-list.css']
})
export class UserListComponent implements OnInit {
  filteredUsers$!: Observable<User[]>;
  searchControl!: FormControl;
  roleFilterControl!: FormControl;
  roles = ['All', 'Admin', 'User', 'Guest'];

  constructor(private userService: UserService) { }

  ngOnInit(): void {
    this.filteredUsers$ = this.userService.filteredUsers$;
    this.searchControl = this.userService.searchControl;
    this.roleFilterControl = this.userService.roleFilterControl;
  }
}