import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { catchError, Observable, throwError } from "../../../node_modules/rxjs/dist/types";

@Injectable()
export class ProductService{
    private http = inject(HttpClient);

    getProduct(id:number=1){
        return this.http.get('https://dummyjson.com/products/'+id)
    }

    getAllProducts():Observable<any[]>{
        return this.http.get<any[]>('https://dummyjson.com/products');
    }
}