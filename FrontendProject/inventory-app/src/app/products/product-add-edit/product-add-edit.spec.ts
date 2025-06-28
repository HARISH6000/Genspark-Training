import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductAddEdit } from './product-add-edit';

describe('ProductAddEdit', () => {
  let component: ProductAddEdit;
  let fixture: ComponentFixture<ProductAddEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductAddEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductAddEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
