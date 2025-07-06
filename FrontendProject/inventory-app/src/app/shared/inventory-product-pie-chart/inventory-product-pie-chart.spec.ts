import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InventoryProductPieChartComponent } from './inventory-product-pie-chart';

describe('InventoryProductPieChartComponent', () => {
  let component: InventoryProductPieChartComponent;
  let fixture: ComponentFixture<InventoryProductPieChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InventoryProductPieChartComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InventoryProductPieChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
