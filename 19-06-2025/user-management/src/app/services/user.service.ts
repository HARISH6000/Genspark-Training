import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest, debounceTime, distinctUntilChanged, map, startWith } from 'rxjs';
import { User } from '../models/user.model';
import { FormControl } from '@angular/forms'; 

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private usersSubject = new BehaviorSubject<User[]>([]);
  users$ = this.usersSubject.asObservable();

  searchControl = new FormControl('');
  roleFilterControl = new FormControl('All');

  private dummyUsers: User[] = [
    { id: '1', username: 'john_doe', email: 'john@example.com', role: 'User' },
    { id: '2', username: 'admin_user', email: 'admin@example.com', role: 'Admin' },
    { id: '3', username: 'guest_account', email: 'guest@example.com', role: 'Guest' },
    { id: '4', username: 'jane_smith', email: 'jane@example.com', role: 'User' },
    { id: '5', username: 'super_admin', email: 'super@example.com', role: 'Admin' },
  ];

  constructor() {
    this.usersSubject.next(this.dummyUsers);
  }

  addUser(user: User): void {
    const currentUsers = this.usersSubject.getValue();
    this.usersSubject.next([...currentUsers, { ...user, id: String(currentUsers.length + 1) }]);
  }

  get filteredUsers$(): Observable<User[]> {
    return combineLatest([
      this.users$,
      this.searchControl.valueChanges.pipe(
        startWith(''),
        debounceTime(300),
        distinctUntilChanged(),
        map(term => term || '')
      ),
      this.roleFilterControl.valueChanges.pipe(
        startWith('All'),
        distinctUntilChanged()
      )
    ]).pipe(
      map(([users, searchTerm, roleFilter]) => {
        let filtered = users;

        if (searchTerm) {
          filtered = filtered.filter(user =>
            user.username.toLowerCase().includes(searchTerm.toLowerCase()) ||
            user.role.toLowerCase().includes(searchTerm.toLowerCase())
          );
        }

        if (roleFilter && roleFilter !== 'All') {
          filtered = filtered.filter(user => user.role === roleFilter);
        }

        return filtered;
      })
    );
  }
}