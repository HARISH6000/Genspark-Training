import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InventoryManagersBarChart } from './inventory-managers-bar-chart';

describe('InventoryManagersBarChart', () => {
  let component: InventoryManagersBarChart;
  let fixture: ComponentFixture<InventoryManagersBarChart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InventoryManagersBarChart]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InventoryManagersBarChart);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
