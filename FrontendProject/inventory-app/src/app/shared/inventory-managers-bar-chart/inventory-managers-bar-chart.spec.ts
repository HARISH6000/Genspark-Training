import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InventoryManagersBarChartComponent } from './inventory-managers-bar-chart';

describe('InventoryManagersBarChartComponent', () => {
  let component: InventoryManagersBarChartComponent;
  let fixture: ComponentFixture<InventoryManagersBarChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InventoryManagersBarChartComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InventoryManagersBarChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
