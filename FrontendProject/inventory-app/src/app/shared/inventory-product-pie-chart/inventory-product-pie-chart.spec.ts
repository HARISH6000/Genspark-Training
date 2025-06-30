import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InventoryProductPieChart } from './inventory-product-pie-chart';

describe('InventoryProductPieChart', () => {
  let component: InventoryProductPieChart;
  let fixture: ComponentFixture<InventoryProductPieChart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InventoryProductPieChart]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InventoryProductPieChart);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
