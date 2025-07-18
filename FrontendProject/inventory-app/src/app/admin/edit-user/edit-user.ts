import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, UserDetails } from '../../services/auth.service';
import { UserService} from '../../services/user.service';
import { User,Role, UpdateUserRequest} from '../../models/user';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-edit-user',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-user.html',
  styleUrl: './edit-user.css'
})
export class EditUserComponent implements OnInit {
  editingUserId: number | null = null; 
  userToEdit: User | null = null;      
  currentUser: UserDetails | null = null;
  roles: Role[] = [];                  
  selectedFile: File | null = null;

  loading: boolean = true;       
  saving: boolean = false;       
  uploadingPicture: boolean = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  isCurrentUserAdmin: boolean = false;
  isEditingOwnProfile: boolean = false;

  
  dummyProfilePictureUrl: string = 'https://placehold.co/150x150/EEEEEE/333333?text=User';

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.currentUserValue; 

    
    this.isCurrentUserAdmin = this.currentUser?.roleName === 'Admin';

    
    this.activatedRoute.paramMap.subscribe(params => {
      const userIdParam = params.get('userId');

      if (userIdParam) {
        this.editingUserId = +userIdParam; 
        this.isEditingOwnProfile = (this.currentUser?.userId === this.editingUserId);
        this.fetchUserToEdit(this.editingUserId);
      } else {
        this.errorMessage = 'User ID not provided in the route.';
        this.loading = false;
      }
    });

    if (this.isCurrentUserAdmin) {
      this.fetchRoles();
    }
  }

  fetchUserToEdit(userId: number): void {
    this.loading = true;
    this.errorMessage = null;
    this.userService.getUserById(userId).pipe(
      catchError(error => {
        this.errorMessage = error.message || 'Failed to load user details.';
        this.loading = false;
        return of(null);
      })
    ).subscribe(user => {
      this.userToEdit = user;
      this.loading = false;
    });
  }

  
  fetchRoles(): void {
    this.userService.getAllRoles().pipe(
      catchError(error => {
        console.warn('Could not fetch roles:', error);
        return of([]);
      })
    ).subscribe(roles => {
      this.roles = roles;
    });
  }

  
  get canEditUsername(): boolean {
    return this.isCurrentUserAdmin;
  }

  
  get canEditRoleId(): boolean {
    return this.isCurrentUserAdmin;
  }

  get canChangeProfile(): boolean{
    return this.isEditingOwnProfile;
  }

  onSubmit(): void {
    if (!this.userToEdit || !this.editingUserId) {
      this.errorMessage = 'User data not loaded or ID missing.';
      return;
    }

    this.saving = true;
    this.errorMessage = null;
    this.successMessage = null;

    const updateRequest: UpdateUserRequest = {
      username: this.userToEdit.username,
      email: this.userToEdit.email,
      phone: this.userToEdit.phone,
      roleId: this.userToEdit.roleId
    };

    
    if (!this.isCurrentUserAdmin) {
      const originalUser = this.userToEdit;
      updateRequest.username = originalUser.username;
      updateRequest.roleId = originalUser.roleId;
    }

    this.userService.updateUser(this.userToEdit.userId,updateRequest).pipe(
      finalize(() => this.saving = false),
      catchError(error => {
        this.errorMessage = error.message || 'An error occurred while updating user details.';
        return of(null);
      })
    ).subscribe(updatedUser => {
      if (updatedUser) {
        this.userToEdit = updatedUser;
        this.successMessage = 'User details updated successfully!';
      }
    });
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    } else {
      this.selectedFile = null;
    }
  }

  uploadProfilePicture(): void {
    if (!this.selectedFile || !this.editingUserId) {
      this.errorMessage = 'Please select a file to upload.';
      return;
    }

    this.uploadingPicture = true;
    this.errorMessage = null;
    this.successMessage = null;

    this.userService.uploadProfilePicture(this.editingUserId, this.selectedFile).pipe(
      finalize(() => {
        this.uploadingPicture = false;
        this.selectedFile = null;
      }),
      catchError(error => {
        console.error('Error segment of component:', error);
        this.errorMessage = error.message || 'Failed to upload profile picture.';
        return of(null);
      })
    ).subscribe((res:any) => {
      if (!res) {
        return;
      }
      console.log('Profile picture upload response:', res);
      this.successMessage = res?.message || 'Profile picture uploaded successfully!';
      if (this.editingUserId) {
        this.fetchUserToEdit(this.editingUserId);
      }
      this.authService.getUserDetails(this.userToEdit?.username || '').subscribe();
    });
  }

  getProfilePictureUrl(): string {
    return this.userToEdit?.profilePictureUrl && this.userToEdit.profilePictureUrl.length > 0
      ? this.userToEdit.profilePictureUrl
      : this.dummyProfilePictureUrl;
  }
}