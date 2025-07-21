import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { VideoListComponent } from './components/video-list/video-list';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    VideoListComponent
  ],
  templateUrl: 'app.html'
})
export class App { }
