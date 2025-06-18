import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService, User } from '../../services/user.service';
import { GenderPieChartComponent } from '../gender-pie-chart/gender-pie-chart.component';
import { RoleBarChartComponent } from '../role-bar-chart/role-bar-chart.component';


@Component({
  selector: 'app-user-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
     GenderPieChartComponent,
    RoleBarChartComponent 
  ],
  templateUrl: './user-dashboard.component.html',
  styleUrls: ['./user-dashboard.component.css']
})
export class UserDashboardComponent implements OnInit {
  allUsers: User[] = [];
  filteredUsers: User[] = [];
  roles: string[] = [];
  genders: string[] = ['Male', 'Female'];
  states: string[] = []; 

  selectedRole: string = '';
  selectedGender: string = '';


  constructor(private userService: UserService) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.userService.getAllUsers().subscribe({
      next: (response) => {
        this.allUsers = response.users.map(user => ({
          ...user,
          role: user.role || 'Unspecified',
          gender: user.gender || 'Unknown',
          address: { 
            address: user.address?.address || '',
            city: user.address?.city || '',
            state: user.address?.state || 'N/A',
            stateCode: user.address?.stateCode || '',
            postalCode: user.address?.postalCode || '',
            coordinates: user.address?.coordinates || {lat: 0, lng: 0},
            country: user.address?.country || ''
          }
        }));
        console.log(this.allUsers);
        this.filteredUsers = [...this.allUsers];
        this.extractUniqueRolesAndStates();
      },
      error: (err) => {
        console.error('Error fetching users:', err);
      }
    });
  }

  extractUniqueRolesAndStates(): void {
    const rolesSet = new Set<string>();
    const statesSet = new Set<string>();
    this.allUsers.forEach(user => {
      if (user.role) {
        rolesSet.add(user.role);
      }
      if (user.address?.state) {
        statesSet.add(user.address.state);
      }
    });
    this.roles = Array.from(rolesSet).sort();
    this.states = Array.from(statesSet).sort();
  }

  applyFilters(): void {
    this.filteredUsers = this.allUsers.filter(user => {
      const matchesRole = this.selectedRole ? user.role?.toLowerCase() === this.selectedRole.toLowerCase() : true;
      const matchesGender = this.selectedGender ? user.gender?.toLowerCase() === this.selectedGender.toLowerCase() : true;
      return matchesRole && matchesGender;
    });
  }

  clearFilters(): void {
    this.selectedRole = '';
    this.selectedGender = '';
    this.filteredUsers = [...this.allUsers];
  }
  
}