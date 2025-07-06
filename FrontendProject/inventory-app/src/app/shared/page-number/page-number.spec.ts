import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PageNumberComponent } from './page-number';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

describe('PageNumberComponent', () => {
  let component: PageNumberComponent;
  let fixture: ComponentFixture<PageNumberComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        PageNumberComponent,
        FormsModule,
        CommonModule
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PageNumberComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(component.totalPages).toBe(1);
    expect(component.pageNumber).toBe(1);
    expect(component.boundaryCount).toBe(2);
    expect(component.siblingCount).toBe(1);
  });

  describe('goToPage', () => {
    beforeEach(() => {
      component.totalPages = 10;
      component.pageNumber = 5;
      fixture.detectChanges();
      spyOn(component.pageChange, 'emit');
    });

    it('should update pageNumber and emit event for valid page', () => {
      component.goToPage(3);
      expect(component.pageNumber).toBe(3);
      expect(component.pageChange.emit).toHaveBeenCalledWith(3);
    });

    it('should not update pageNumber or emit event for page less than 1', () => {
      component.goToPage(0);
      expect(component.pageNumber).toBe(5);
      expect(component.pageChange.emit).not.toHaveBeenCalled();
    });

    it('should not update pageNumber or emit event for page greater than totalPages', () => {
      component.goToPage(11);
      expect(component.pageNumber).toBe(5);
      expect(component.pageChange.emit).not.toHaveBeenCalled();
    });

    it('should handle string input for page number', () => {
      component.goToPage('7' as any);
      expect(component.pageNumber).toBe(7);
      expect(component.pageChange.emit).toHaveBeenCalledWith(7);
    });
  });

  describe('pagesToShow getter', () => {
    it('should return correct pages when totalPages is small (e.g., 5)', () => {
      component.totalPages = 5;
      component.pageNumber = 3;
      fixture.detectChanges();

      const pages = component.pagesToShow;
      expect(pages.firstPages).toEqual([1, 2]);
      expect(pages.middlePages).toEqual([3]);
      expect(pages.lastPages).toEqual([4, 5]);
    });

    it('should return correct pages when totalPages is large and current page is in the middle', () => {
      component.totalPages = 20;
      component.pageNumber = 10;
      fixture.detectChanges();

      const pages = component.pagesToShow;
      expect(pages.firstPages).toEqual([1, 2]);
      expect(pages.middlePages).toEqual([9, 10, 11]);
      expect(pages.lastPages).toEqual([19, 20]);
    });

    it('should return correct pages when current page is near the beginning', () => {
      component.totalPages = 20;
      component.pageNumber = 3;
      fixture.detectChanges();

      const pages = component.pagesToShow;
      expect(pages.firstPages).toEqual([1, 2]);
      expect(pages.middlePages).toEqual([3, 4]);
      expect(pages.lastPages).toEqual([19, 20]);
    });

    it('should return correct pages when current page is near the end', () => {
      component.totalPages = 20;
      component.pageNumber = 18;
      fixture.detectChanges();

      const pages = component.pagesToShow;
      expect(pages.firstPages).toEqual([1, 2]);
      expect(pages.middlePages).toEqual([17, 18]);
      expect(pages.lastPages).toEqual([19, 20]);
    });

    it('should handle totalPages equal to boundaryCount * 2 + siblingCount', () => {
      component.totalPages = 5;
      component.pageNumber = 3;
      fixture.detectChanges();

      const pages = component.pagesToShow;
      expect(pages.firstPages).toEqual([1, 2]);
      expect(pages.middlePages).toEqual([3]);
      expect(pages.lastPages).toEqual([4, 5]);
    });
  });

  describe('ellipsis getters', () => {
    it('should show left ellipsis when pageNumber is far from beginning', () => {
      component.totalPages = 20;
      component.pageNumber = 5;
      fixture.detectChanges();
      expect(component.showLeftEllipsis).toBeTrue();
    });

    it('should not show left ellipsis when pageNumber is close to beginning', () => {
      component.totalPages = 20;
      component.pageNumber = 3;
      fixture.detectChanges();
      expect(component.showLeftEllipsis).toBeFalse();
    });

    it('should show right ellipsis when pageNumber is far from end', () => {
      component.totalPages = 20;
      component.pageNumber = 16;
      fixture.detectChanges();
      expect(component.showRightEllipsis).toBeTrue();
    });

    it('should not show right ellipsis when pageNumber is close to end', () => {
      component.totalPages = 20;
      component.pageNumber = 18;
      fixture.detectChanges();
      expect(component.showRightEllipsis).toBeFalse();
    });

    it('should not show either ellipsis if totalPages is small', () => {
      component.totalPages = 5;
      component.pageNumber = 3;
      fixture.detectChanges();
      expect(component.showLeftEllipsis).toBeFalse();
      expect(component.showRightEllipsis).toBeFalse();
    });

    it('should not show either ellipsis if boundaryCount equals totalPages - boundaryCount', () => {
      component.totalPages = 4;
      component.boundaryCount = 2;
      component.pageNumber = 2;
      fixture.detectChanges();
      expect(component.showLeftEllipsis).toBeFalse();
      expect(component.showRightEllipsis).toBeFalse();
    });
  });
});
