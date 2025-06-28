import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PageNumber } from './page-number';

describe('PageNumber', () => {
  let component: PageNumber;
  let fixture: ComponentFixture<PageNumber>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PageNumber]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PageNumber);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
