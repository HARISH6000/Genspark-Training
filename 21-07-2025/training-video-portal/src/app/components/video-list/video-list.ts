import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { VideoService } from '../../services/video.service';
import { UploadDialogComponent } from '../upload-dialog/upload-dialog';
import { PlayerDialogComponent } from '../player-dialog/player-dialog';

@Component({
  selector: 'app-video-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatDialogModule,
    UploadDialogComponent,
    PlayerDialogComponent
  ],
  templateUrl: './video-list.html',
  styles: [`.video-list { padding: 20px; } .video-card { margin-top: 15px; }`]
})
export class VideoListComponent implements OnInit {
  private videoService = inject(VideoService);
  private dialog = inject(MatDialog);

  videos: any[] = [];

  ngOnInit() {
    this.videoService.getVideos().subscribe(videos => this.videos = videos);
  }

  openUploadDialog() {
    const dialogRef = this.dialog.open(UploadDialogComponent);
    dialogRef.afterClosed().subscribe(result => {
      if (result === 'refresh') {
        this.videoService.getVideos().subscribe(videos => this.videos = videos);
      }
    });
  }

  openPlayerDialog(blobUrl: string) {
    this.dialog.open(PlayerDialogComponent, {
      data: { blobUrl }
    });
  }
}