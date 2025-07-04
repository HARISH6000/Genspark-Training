import { Routes } from '@angular/router';
import { PaymentFormComponent } from './payment-form/payment-form';
import { PaymentSuccessComponent } from './payment-success/payment-success';

export const routes: Routes = [
  { path: '', component: PaymentFormComponent },
  { path: 'success', component: PaymentSuccessComponent }
];
