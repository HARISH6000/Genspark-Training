import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { ProductService } from '../services/product';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CommonModule, NgIf, NgFor } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

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
export class HomeComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  searchTerm: string = '';
  skip: number = 0;
  limit: number = 12;
  loading: boolean = false;
  totalProducts: number = 0;

  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription | undefined;

  constructor(private productService: ProductService, private router: Router) { }

  ngOnInit(): void {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(500),
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

  onSearchInputChange(event: Event): void {
    console.log(event);
    this.searchSubject.next(event.toString());
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

  viewProductDetails(productId: number): void {
    this.router.navigate(['/products', productId]);
  }

  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}

