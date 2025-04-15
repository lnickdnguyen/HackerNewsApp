import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { StoryListComponent } from './story-list.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, StoryListComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'hacker-news-app';
}
