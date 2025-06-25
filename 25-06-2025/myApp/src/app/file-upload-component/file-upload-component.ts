import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BulkInsertService } from '../services/BulkInsertService'
import { JsonPipe } from '@angular/common';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload-component.html',
  imports:[JsonPipe]
})
export class FileUploadComponent {
  constructor(private http: HttpClient) {}
 private service =  inject(BulkInsertService);
  insertedRecords:any;

  handleFileUpload(event: any) {
  const file = event.target.files[0];
  this.service.processData(file).subscribe({
    next:(data)=>this.insertedRecords= data,
    error:(err)=>alert(err)

  })
  

  }
  

}