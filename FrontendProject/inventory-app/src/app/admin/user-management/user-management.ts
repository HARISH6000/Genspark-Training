import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, UserDetails } from '../../services/auth.service';
import { User } from '../../models/user';
import { Role } from '../../models/user';
import { UserService } from '../../services/user.service';
import { PaginationResponse } from '../../models/pagination-response';
import { SortCriterion } from '../../models/sortCriterion';
import { PageNumberComponent } from '../../shared/page-number/page-number';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { BehaviorSubject, Observable, combineLatest, debounceTime, distinctUntilChanged, map, startWith } from 'rxjs';


@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, PageNumberComponent],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css'
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  roles:Role[] = [];
  selectedRole:number | null = null;
  pageNumber: number = 1;
  totalPages: number = 1;
  pageSize: number = 5;
  searchTerm: string | null = null;
  loading: boolean = true;
  errorMessage: string | null = null;
  currentUser: User | null = null;
  filteredUsers: User[] = [];
  searchControl = new FormControl('');

  sortFields = [
    { value: 'username', name: 'Name' },
    { value: 'roleName', name: 'Role' },
    { value: 'userId', name: 'User Id' }
  ];
  sortOrders = [
    { value: 'asc', name: 'Ascending (A-Z, 0-9)' },
    { value: 'desc', name: 'Descending (Z-A, 9-0)' }
  ];
  sortCriteria: SortCriterion[] = [];


  dummyProfilePictureUrl: string = 'https://placehold.co/100x100/EEEEEE/333333?text=UserDetails';

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {

    this.currentUser = this.authService.currentUserValue;

    const sortByParam = this.buildSortByString();
    console.log("srtParam:", sortByParam);

    this.fetchAllRoles();

    this.searchControl.valueChanges.pipe(startWith(''),
      debounceTime(300),
      distinctUntilChanged(),
      map(term => term || '')
    ).subscribe(value => {
      this.pageNumber=1;
      this.onSearchInputChange(value);
    });

    if (this.currentUser && this.currentUser.roleName === 'Admin') {
      this.fetchAllUsers();
    } else {
      this.loading = false;
      this.errorMessage = 'Access Denied: You must be an administrator to view this page.';
      this.router.navigate(['/dashboard']);
    }
  }

  fetchAllRoles():void{
    this.loading =true;
    this.errorMessage=null;

    this.userService.getAllRoles().pipe(catchError(error => {
        console.error('Error fetching roles:', error);
        this.errorMessage = error.message || 'An error occurred while fetching roles.';
        this.loading = false;
        return [];
      })).subscribe((response:Role[])=>{
        console.log("roles:",response);
        this.roles=response;
      });
  }

  
  fetchAllUsers(): void {
    this.loading = true;
    this.errorMessage = null;

    const sortByParam = this.buildSortByString();
    console.log("srtParam:", sortByParam);

    this.userService.getAllUsers(this.pageNumber, this.pageSize, this.searchControl.value, sortByParam).pipe(
      catchError(error => {
        console.error('Error fetching users:', error);
        this.errorMessage = error.message || 'An error occurred while fetching users.';
        this.loading = false;
        return of({
          data: [],
          pagination: {
            totalRecords: 0,
            page: 0,
            pageSize: 0,
            totalPages: 0,
          },
        } as PaginationResponse<User>);
      })
    ).subscribe((response: PaginationResponse<User>) => {
      this.users = response.data;
      this.totalPages=response.pagination.totalPages;
      this.pageNumber=response.pagination.page;
      this.applyRoleFilter();
      //this.filteredUsers=response.data;
      this.loading = false;
      console.log("usrs:", this.users);
    });
    this.userService.getUserById(1).subscribe((data: UserDetails) => { console.log("user_1:", data) });
    this.userService.getUserByUserName("Bob").subscribe((data: UserDetails) => { console.log("user_2:", data) });
  }

  /**
   * Navigates to a specific user's info page.
   * @param user The user object to navigate to.
   */
  viewUserInfo(user: UserDetails): void {
    this.router.navigate(['/user-info', user.userId], { state: { user: user } });
  }


  getProfilePictureUrl(user: UserDetails): string {
    return user.profilePictureUrl && user.profilePictureUrl.length > 0
      ? user.profilePictureUrl
      : this.dummyProfilePictureUrl;
  }

  private buildSortByString(): string {
    return this.sortCriteria
      .filter(c => c.field && c.order)
      .map(c => `${c.field}_${c.order}`)
      .join(',');
  }

  applyRoleFilter(): void {
    let tempUsers = [...this.users];
    if (this.selectedRole!==null && this.selectedRole>0) {
      console.log("in:",this.selectedRole);
      tempUsers = tempUsers.filter(user =>
        user.roleId==this.selectedRole
      );
    }
    this.filteredUsers = tempUsers;
  }


  addSortCriterion(): void {
    this.sortCriteria.push({ field: '', order: 'asc' });
  }

  removeSortCriterion(index: number): void {
    this.sortCriteria.splice(index, 1);
    this.onSortChange();
  }


  onSearchInputChange(value: any): void {
    this.fetchAllUsers();
  }

  onSortChange(): void {
    this.fetchAllUsers();
  }

  onRoleChange(): void {
    this.applyRoleFilter();
  }
  onPageChange(page:number):void{
    if(page>=1 && page<=this.totalPages){
      this.pageNumber=page;
      this.fetchAllUsers();
    }
  }

  onPageSizeChange(): void {
    this.pageNumber = 1;
    this.fetchAllUsers();
  }
}