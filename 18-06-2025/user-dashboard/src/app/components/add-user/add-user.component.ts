import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService, User } from '../../services/user.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-add-user',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
    ],
    templateUrl: './add-user.component.html',
    styleUrls: ['./add-user.component.css']
})
export class AddUserComponent implements OnInit {
    allUsers: User[] = [];
    roles: string[] = [];
    genders: string[] = ['male', 'female', 'other'];
    states: string[] = [];


    newUser: Partial<User> = {
        firstName: '',
        lastName: '',
        gender: 'male',
        age: 0,
        role: 'user',
        address: {
            address: '',
            city: '',
            state: '',
            stateCode: '',
            postalCode: '',
            coordinates: {
                lat: 0,
                lng: 0,
            },
            country: ''
        }
    }


    constructor(private userService: UserService,private route:Router) { }

    ngOnInit(): void {

    }

    addUser(): void {
        this.userService.addUser(this.newUser).subscribe({
            next:(addedUser)=>{
                console.log("user added:",addedUser);
            }
        });
        this.route.navigateByUrl('/dashboard');
    }



}