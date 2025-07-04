import { Component, OnInit, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { environment } from '../../environments/environment.development';
import { Router } from '@angular/router';

declare var Razorpay: any;

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './payment-form.html',
  styleUrls: ['./payment-form.css']
})
export class PaymentFormComponent implements OnInit {
  paymentForm!: FormGroup;
  razorpayKeyId: string = environment.razorpayKeyId;
  paymentStatus: string | null = null;
  paymentId: string | null = null;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.paymentForm = this.fb.group({
      amount: ['', [Validators.required, Validators.min(1)]],
      customerName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      contactNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]]
    });
  }

  onSubmit(): void {
    this.paymentStatus = null;
    this.paymentId = null;

    if (this.paymentForm.valid) {
      const { amount, customerName, email, contactNumber } = this.paymentForm.value;

      const options = {
        key: this.razorpayKeyId,
        amount: amount * 100, // in paise
        currency: 'INR',
        name: 'Razorpay Demo',
        description: 'Test Payment',
        handler: (response: any) => {
          this.ngZone.run(() => {
            console.log('Payment successful:', response);
            this.paymentStatus = 'success';
            this.paymentId = response.razorpay_payment_id;
            this.router.navigate(['/success']);
          });
        },
        prefill: {
          name: customerName,
          email: email,
          contact: contactNumber
        },
        notes: {
          address: 'Razorpay Office'
        },
        theme: {
          color: '#3399CC'
        }
      };

      const rzp = new Razorpay(options);

      rzp.on('payment.failed', (response: any) => {
        this.ngZone.run(() => {
          console.error('Payment failed:', response);
          this.paymentStatus = 'failed';
          this.paymentId = response.error?.metadata?.payment_id || null;
        });
      });

      rzp.on('payment.cancelled', () => {
        this.ngZone.run(() => {
          console.warn('Payment cancelled by user');
          this.paymentStatus = 'cancelled';
          this.paymentId = null;
        });
      });

      rzp.open();

    } else {
      this.paymentForm.markAllAsTouched();
    }
  }

  get f() {
    return this.paymentForm.controls;
  }
}
