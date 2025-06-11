import { Component, inject } from '@angular/core';
import { ProductService } from '../services/product.service';
import { ProductModel } from '../models/product';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-product',
  imports: [CurrencyPipe],
  templateUrl: './product.component.html',
  styleUrl: './product.component.css'
})

export class Product {
  product:ProductModel|null = new ProductModel();
  private productService = inject(ProductService);

  constructor(){
      this.productService.getProduct(1).subscribe(
        {
          next:(data)=>{
      
            this.product = data as ProductModel;
            console.log(this.product)
          },
          error:(err)=>{
            console.log(err)
          },
          complete:()=>{
            console.log("All done");
          }
        })
  }

}