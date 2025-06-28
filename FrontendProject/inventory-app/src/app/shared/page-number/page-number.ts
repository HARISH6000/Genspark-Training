import { Component, Input, Output, EventEmitter } from '@angular/core'
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './page-number.html',
  styleUrls: ['./page-number.css']
})
export class PageNumberComponent {
  @Input() totalPages: number = 1;
  @Input() pageNumber: number = 1;
  @Output() pageChange = new EventEmitter<number>();
  boundaryCount:number = 2;
  siblingCount:number = 1;

  get pagesToShow() {
    const firstPages = [];
    const lastPages = [];
    const middlePages = [];
    

    // Add first pages
    for (let i = 1; i <= Math.min(this.boundaryCount, this.totalPages); i++) {
      firstPages.push(i);
    }

    // Add last pages
    if (this.totalPages > this.boundaryCount) {
      for (let i = Math.max(this.totalPages - this.boundaryCount + 1, Math.min(this.boundaryCount + 1, this.totalPages)); i <= this.totalPages; i++) {
        lastPages.push(i);
      }
    }


    // Add middle pages around the current page
    const start = Math.max(this.boundaryCount + 1, this.pageNumber - this.siblingCount);
    const end = Math.min(this.totalPages - this.boundaryCount, this.pageNumber + this.siblingCount);
    for (let i = start; i <= end; i++) {
      middlePages.push(i);
    }

    return { firstPages, middlePages, lastPages };
  }

  get showLeftEllipsis() {
    if(this.boundaryCount===this.totalPages-this.boundaryCount){
      return false;
    }
    return this.pageNumber > this.boundaryCount + this.siblingCount + 1;
  }
  
  get showRightEllipsis() {
    if(this.boundaryCount===this.totalPages-this.boundaryCount){
      return false;
    }
    return this.pageNumber < this.totalPages - this.boundaryCount - this.siblingCount;
  }

  goToPage(page: number): void {
    const pageNum = Number(page); // Ensure number
    if (pageNum >= 1 && pageNum <= this.totalPages) {
      this.pageNumber = pageNum;
      // Emit an event or perform action for page change
      console.log(`Navigated to page: ${pageNum}`);
      this.pageChange.emit(pageNum);
    }
  }
}
