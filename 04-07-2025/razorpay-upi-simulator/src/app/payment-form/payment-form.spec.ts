import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaymentFormComponent } from './payment-form';
import { ReactiveFormsModule } from '@angular/forms';
import { provideRouter } from '@angular/router';
import { Location } from '@angular/common';
import { Routes } from '@angular/router';
import { PaymentSuccessComponent } from '../payment-success/payment-success';
import { Router } from '@angular/router';
import { By } from '@angular/platform-browser';

const routes: Routes = [
  { path: 'success', component: PaymentSuccessComponent }
];

describe('PaymentFormComponent', () => {
  let component: PaymentFormComponent;
  let fixture: ComponentFixture<PaymentFormComponent>;
  let router: Router;
  let location: Location;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaymentFormComponent, ReactiveFormsModule],
      providers: [provideRouter(routes)]
    }).compileComponents();

    router = TestBed.inject(Router);
    location = TestBed.inject(Location);
    fixture = TestBed.createComponent(PaymentFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    await router.initialNavigation();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  describe('Form Validation', () => {
    it('should be invalid when empty', () => {
      expect(component.paymentForm.valid).toBeFalse();
    });

    it('should be valid with proper inputs', () => {
      component.paymentForm.setValue({
        amount: 100,
        customerName: 'John Doe',
        email: 'john@example.com',
        contactNumber: '9876543210'
      });
      expect(component.paymentForm.valid).toBeTrue();
    });

    it('should mark controls as invalid with wrong data', () => {
      component.paymentForm.setValue({
        amount: 0,
        customerName: '',
        email: 'bademail',
        contactNumber: 'abc123'
      });

      const f = component.f;
      expect(f['amount'].valid).toBeFalse();
      expect(f['customerName'].valid).toBeFalse();
      expect(f['email'].valid).toBeFalse();
      expect(f['contactNumber'].valid).toBeFalse();
    });
  });

  describe('Razorpay Integration', () => {
    let razorpayMock: any;

    beforeEach(() => {
      
      razorpayMock = jasmine.createSpyObj('Razorpay', ['open', 'on']);
      (window as any).Razorpay = jasmine.createSpy().and.returnValue(razorpayMock);
    });

    it('should call Razorpay when form is valid and submitted', () => {
      spyOn(router, 'navigate');

      component.paymentForm.setValue({
        amount: 500,
        customerName: 'Alice',
        email: 'alice@example.com',
        contactNumber: '9999999999'
      });

      component.onSubmit();

      expect((window as any).Razorpay).toHaveBeenCalled();
      expect(razorpayMock.open).toHaveBeenCalled();
    });

    it('should not call Razorpay when form is invalid', () => {
      component.paymentForm.setValue({
        amount: 0,
        customerName: '',
        email: '',
        contactNumber: ''
      });

      component.onSubmit();

      expect((window as any).Razorpay).not.toHaveBeenCalled();
    });
  });

  describe('Template Rendering', () => {
    it('should render form title', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.card-header h3')?.textContent).toContain('UPI Payment Simulator');
    });

    it('should disable submit button if form is invalid', () => {
      const button = fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement as HTMLButtonElement;
      expect(button.disabled).toBeTrue();
    });

    it('should enable submit button if form is valid', () => {
      component.paymentForm.setValue({
        amount: 500,
        customerName: 'Valid Name',
        email: 'valid@email.com',
        contactNumber: '9999999999'
      });
      fixture.detectChanges();
      const button = fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement as HTMLButtonElement;
      expect(button.disabled).toBeFalse();
    });
  });
});
