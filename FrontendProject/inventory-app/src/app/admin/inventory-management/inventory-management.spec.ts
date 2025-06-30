import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InventoryManagement } from './inventory-management';

describe('InventoryManagement', () => {
  let component: InventoryManagement;
  let fixture: ComponentFixture<InventoryManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InventoryManagement]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InventoryManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
