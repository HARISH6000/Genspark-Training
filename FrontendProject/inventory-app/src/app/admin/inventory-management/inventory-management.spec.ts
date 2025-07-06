import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { InventoryManagementComponent } from './inventory-management';
import { Router } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';
import { AuthService, UserDetails } from '../../services/auth.service';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { Inventory, CreateInventoryRequest, UpdateInventoryRequest } from '../../models/inventory';
import { PaginationResponse } from '../../models/pagination-response';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PageNumberComponent } from '../../shared/page-number/page-number';

interface MockAuthService extends jasmine.SpyObj<AuthService> {
  currentUser: BehaviorSubject<UserDetails | null>;
  currentUserValue: UserDetails | null;
}

describe('InventoryManagementComponent', () => {
  let component: InventoryManagementComponent;
  let fixture: ComponentFixture<InventoryManagementComponent>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockInventoryService: jasmine.SpyObj<InventoryService>;
  let mockAuthService: MockAuthService;

  const mockInventories: Inventory[] = [
    { inventoryId: 1, name: 'Warehouse A', location: 'Location A', isDeleted: false },
    { inventoryId: 2, name: 'Warehouse B', location: 'Location B', isDeleted: false },
  ];

  const mockPaginationResponse: PaginationResponse<Inventory> = {
    data: mockInventories,
    pagination: {
      totalRecords: 2,
      page: 1,
      pageSize: 10,
      totalPages: 1,
    },
  };

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockInventoryService = jasmine.createSpyObj('InventoryService', [
      'getAllInventories', 'createInventory', 'updateInventory',
      'softDeleteInventory', 'hardDeleteInventory'
    ]);
    mockAuthService = jasmine.createSpyObj<AuthService>('AuthService', ['isAdmin', 'isManager', 'currentUserValue']) as MockAuthService;

    mockInventoryService.getAllInventories.and.returnValue(of(mockPaginationResponse));
    mockInventoryService.createInventory.and.returnValue(of({ ...mockInventories[0], inventoryId: 3 }));
    mockInventoryService.updateInventory.and.returnValue(of(mockInventories[0]));
    mockInventoryService.softDeleteInventory.and.returnValue(of(mockInventories[0]));
    mockInventoryService.hardDeleteInventory.and.returnValue(of(mockInventories[0]));

    mockAuthService.isAdmin.and.returnValue(true);
    mockAuthService.isManager.and.returnValue(false);
    mockAuthService.currentUser = new BehaviorSubject<UserDetails | null>(null);
    Object.defineProperty(mockAuthService, 'currentUserValue', {
      configurable: true,
      get: () => mockAuthService.currentUser.value
    });

    await TestBed.configureTestingModule({
      imports: [
        InventoryManagementComponent,
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        PageNumberComponent,
      ],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: InventoryService, useValue: mockInventoryService },
        { provide: AuthService, useValue: mockAuthService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(InventoryManagementComponent);
    component = fixture.componentInstance;
  });

  it('should create and load inventories on ngOnInit', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);
    expect(component).toBeTruthy();
    expect(mockInventoryService.getAllInventories).toHaveBeenCalled();
    expect(component.inventories).toEqual(mockInventories);
    expect(component.loading).toBeFalse();
  }));

  it('should add a new inventory successfully', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);
    component.openAddForm();
    component.newInventory = { name: 'New Warehouse', location: 'New Location' };
    spyOn(component, 'fetchInventories');
    component.submitForm();
    tick();
    expect(mockInventoryService.createInventory).toHaveBeenCalledWith({ name: 'New Warehouse', location: 'New Location' });
    expect(component.showForm).toBeFalse();
    expect(component.fetchInventories).toHaveBeenCalled();
  }));

  it('should soft delete an inventory and refresh the list', fakeAsync(() => {
    fixture.detectChanges();
    tick(300);
    spyOn(window, 'confirm').and.returnValue(true);
    spyOn(component, 'fetchInventories');
    component.softDeleteInventory(1);
    tick();
    expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to soft delete this inventory? It can be restored later.');
    expect(mockInventoryService.softDeleteInventory).toHaveBeenCalledWith(1);
    expect(component.fetchInventories).toHaveBeenCalled();
  }));
});
