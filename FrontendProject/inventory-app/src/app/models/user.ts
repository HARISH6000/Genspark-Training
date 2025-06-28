export interface User {
  userId: number;
  username: string;
  email: string;
  phone: string;
  profilePictureUrl: string; 
  roleId: number;
  roleName: string;
  isDeleted: boolean;
}


export interface RegisterUserRequest {
  username: string;
  password: string;
  email: string;
  phone: string;
  roleId: number;
}


export interface UpdateUserRequest {
  username: string;
  email: string;
  phone: string;
  roleId: number;
}

export interface UpdateContactDetailsRequest{
    email:string;
    phone:string;
}


export interface ChangePasswordRequest {
  userId: number;
  currentPassword?: string;
  newPassword: string;
}

export interface Role {
  roleId: number;
  roleName: string;
  description: string;
}