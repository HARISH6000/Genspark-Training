import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InventoryInfo } from './inventory-info';

describe('InventoryInfo', () => {
  let component: InventoryInfo;
  let fixture: ComponentFixture<InventoryInfo>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InventoryInfo]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InventoryInfo);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
