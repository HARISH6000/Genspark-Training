import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogClose, MatDialogContent, MatDialogTitle, MatDialogActions } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-player-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatButtonModule,
    MatDialogClose
  ],
  templateUrl: './player-dialog.html',
  styleUrls: ['./player-dialog.scss']
})
export class PlayerDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: { blobUrl: string }) {}
}
