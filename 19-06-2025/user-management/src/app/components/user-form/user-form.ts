import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user.model';
import { CustomValidators } from '../../validators/custom-validators';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-form.html',
  styleUrls: ['./user-form.css']
})
export class UserFormComponent implements OnInit {
  userForm!: FormGroup;
  roles = ['Admin', 'User', 'Guest'];

  constructor(private fb: FormBuilder, private userService: UserService, private router: Router) { }

  ngOnInit(): void {
    this.userForm = this.fb.group({
      username: ['', [
        Validators.required,
        CustomValidators.bannedWords(['admin', 'root', 'superuser'])
      ]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        CustomValidators.passwordStrength()
      ]],
      confirmPassword: ['', Validators.required],
      role: ['User', Validators.required]
    }, {
      validators: CustomValidators.passwordMatch('password', 'confirmPassword')
    });
  }

  get f() { return this.userForm.controls; }

  onSubmit(): void {
    if (this.userForm.valid) {
      const newUser: User = {
        id: '',
        username: this.userForm.value.username,
        email: this.userForm.value.email,
        role: this.userForm.value.role
      };
      this.userService.addUser(newUser);
      console.log('User Added:', newUser);
      this.userForm.reset({ role: 'User' });
      this.router.navigate(['/users']);
      alert('User added successfully!');
    } else {
      console.log('Form is invalid');
      this.userForm.markAllAsTouched();
    }
  }

}  