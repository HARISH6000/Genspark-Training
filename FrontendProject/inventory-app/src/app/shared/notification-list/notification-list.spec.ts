import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NotificationListComponent } from './notification-list';
import { SignalrService } from '../../services/signalr.service';
import { NotificationService } from '../../services/notification.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { LowStockNotificationDto } from '../../models/notification';
import { of, BehaviorSubject } from 'rxjs';
import { CommonModule } from '@angular/common';

describe('NotificationListComponent', () => {
  let component: NotificationListComponent;
  let fixture: ComponentFixture<NotificationListComponent>;
  let mockSignalrService: jasmine.SpyObj<SignalrService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockNotifications: LowStockNotificationDto[] = [
    { productId: 1, productName: 'Item A', sku: 'SKU001', currentQuantity: 5, minStockQuantity: 10, inventoryId: 101, inventoryName: 'Whse 1', message: 'Low stock for Item A', timestamp: new Date().toISOString() },
    { productId: 2, productName: 'Item B', sku: 'SKU002', currentQuantity: 2, minStockQuantity: 5, inventoryId: 102, inventoryName: 'Whse 2', message: 'Low stock for Item B', timestamp: new Date().toISOString() },
  ];

  beforeEach(async () => {
    mockSignalrService = jasmine.createSpyObj('SignalrService', ['connectionStatus$']);
    mockNotificationService = jasmine.createSpyObj('NotificationService', ['lowStockNotification$', 'setLowStockNotification']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['isManager']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    mockSignalrService.connectionStatus$ = new BehaviorSubject<boolean>(true).asObservable();
    mockNotificationService.lowStockNotification$ = new BehaviorSubject<LowStockNotificationDto[]>([]).asObservable();
    mockAuthService.isManager.and.returnValue(false);

    await TestBed.configureTestingModule({
      imports: [
        NotificationListComponent,
        CommonModule,
      ],
      providers: [
        { provide: SignalrService, useValue: mockSignalrService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should clear all notifications', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    component.notifications = [...mockNotifications];
    component.clearAllNotifications();
    expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to clear all notifications? This action cannot be undone.');
    expect(component.notifications).toEqual([]);
    expect(mockNotificationService.setLowStockNotification).toHaveBeenCalledWith([]);
    expect(sessionStorage.getItem('lowStockNotifications')).toBeNull();
  });

  it('should clear a single notification', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    component.notifications = [...mockNotifications];
    component.clearNotification(0);
    expect(component.notifications.length).toBe(1);
    expect(component.notifications[0]).toEqual(mockNotifications[1]);
    expect(mockNotificationService.setLowStockNotification).toHaveBeenCalledWith([mockNotifications[1]]);
    expect(sessionStorage.getItem('lowStockNotifications')).toEqual(JSON.stringify([mockNotifications[1]]));
  });

  it('should navigate back on goBack', () => {
    component.goBack();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should display "No Notifications Yet!" when notifications array is empty', () => {
    component.notifications = [];
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.alert-info')?.textContent).toContain('No Notifications Yet!');
  });
});
