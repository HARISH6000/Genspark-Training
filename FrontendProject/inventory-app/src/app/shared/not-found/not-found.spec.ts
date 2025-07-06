import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NotFoundComponent } from './not-found';
import { CommonModule } from '@angular/common';
import { provideRouter } from '@angular/router';

describe('NotFoundComponent', () => {
  let component: NotFoundComponent;
  let fixture: ComponentFixture<NotFoundComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        NotFoundComponent,
        CommonModule
      ],
      providers: [
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NotFoundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should display "404" and "Page Not Found"', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('404');
    expect(compiled.querySelector('h2')?.textContent).toContain('Page Not Found');
  });
});
