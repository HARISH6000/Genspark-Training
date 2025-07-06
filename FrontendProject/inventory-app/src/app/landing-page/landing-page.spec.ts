import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing'; 
import { LandingPageComponent } from './landing-page';

describe('LandingPageComponent', () => {
  let component: LandingPageComponent;
  let fixture: ComponentFixture<LandingPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        RouterLink,
        RouterTestingModule,
        LandingPageComponent
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LandingPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should set currentYear to the current year on initialization', () => {
    const currentYear = new Date().getFullYear();
    expect(component.currentYear).toBe(currentYear);
  });

  it('should have ngOnInit defined', () => {
    expect(component.ngOnInit).toBeDefined();
    expect(typeof component.ngOnInit).toBe('function');
  });

  it('should render currentYear in the template', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const currentYear = new Date().getFullYear();
    expect(compiled.textContent).toContain(currentYear.toString());
  });
});