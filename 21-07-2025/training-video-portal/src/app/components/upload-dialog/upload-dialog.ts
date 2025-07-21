import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef } from '@angular/material/dialog';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { VideoService } from '../../services/video.service';
import { MatDialogTitle, MatDialogContent, MatDialogActions } from '@angular/material/dialog';

@Component({
  selector: 'app-upload-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions
  ],
  templateUrl: './upload-dialog.html',
  styleUrls: ['./upload-dialog.scss']
})
export class UploadDialogComponent {
  private fb = inject(FormBuilder);
  private videoService = inject(VideoService);
  private dialogRef = inject(MatDialogRef<UploadDialogComponent>);

  form: FormGroup = this.fb.group({
    title: ['', Validators.required],
    description: ['']
  });

  selectedFile!: File;

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    this.form.get('title')?.markAsTouched();
  }

  onSubmit() {
    if (this.form.invalid || !this.selectedFile) {
      this.form.markAllAsTouched();
      return;
    }

    const formData = new FormData();
    formData.append('title', this.form.value.title);
    formData.append('description', this.form.value.description);
    formData.append('File', this.selectedFile);

    this.videoService.uploadVideo(formData).subscribe(() => {
      this.dialogRef.close('refresh');
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
