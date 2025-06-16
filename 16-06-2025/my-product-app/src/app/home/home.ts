import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { ProductService } from '../services/product';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CommonModule, NgIf, NgFor } from '@angular/common'; 
import { FormsModule } from '@angular/forms';

interface Product {
  id: number;
  title: string;
  thumbnail: string;
  price: number;
}

@Component({
  selector: 'app-home',
  standalone: true, 
  imports: [CommonModule, FormsModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home implements OnInit, OnDestroy {
  products: Product[] = [];
  searchTerm: string = '';
  skip: number = 0;
  limit: number = 12;
  loading: boolean = false;
  totalProducts: number = 0;

  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription | undefined;

  constructor(private productService: ProductService) { }

  ngOnInit(): void {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(term => {
      this.searchTerm = term;
      this.resetAndFetchProducts();
    });

    this.fetchProducts();
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }

  @HostListener('window:scroll', ['$event'])
  onScroll(event: Event): void {
    const windowHeight = window.innerHeight;
    const documentHeight = document.documentElement.scrollHeight;
    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;

    if (scrollTop + windowHeight >= documentHeight - 200 && !this.loading && this.products.length < this.totalProducts) {
      this.skip += this.limit;
      this.fetchProducts();
    }

    const backToTopButton = document.getElementById('backToTopBtn');
    if (backToTopButton) {
      if (scrollTop > 300) {
        backToTopButton.style.display = 'block';
      } else {
        backToTopButton.style.display = 'none';
      }
    }
  }

  onSearchInputChange(): void {
    console.log(this.searchTerm);
    this.searchSubject.next(this.searchTerm);
  }

  resetAndFetchProducts(): void {
    this.products = [];
    this.skip = 0;
    this.fetchProducts();
  }

  fetchProducts(): void {
    if (this.loading) {
      return;
    }
    this.loading = true;

    this.productService.getProducts(this.searchTerm, this.limit, this.skip)
      .subscribe({
        next: (data: any) => {
          this.products = [...this.products, ...data.products];
          this.totalProducts = data.total;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error fetching products:', err);
          this.loading = false;
        }
      });
  }

  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
