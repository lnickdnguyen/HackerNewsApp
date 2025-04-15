import { Component, OnInit, NgModule } from '@angular/core';
import { StoryService, Story } from './story-service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-story-list',
  templateUrl: './story-list.component.html',
  styleUrls: ['./story-list.component.css'],
  imports: [CommonModule, FormsModule],
})
export class StoryListComponent implements OnInit {
  stories: Story[] = [];
  page = 1;
  pageSize = 20;
  searchTerm = '';
  loading = false;

  constructor(private storyService: StoryService) {}

  ngOnInit(): void {
    this.loadStories();
  }

  loadStories(): void {
    this.loading = true;

    this.storyService.getStories(this.page, this.pageSize, this.searchTerm).subscribe({
      next: (data) => {
        this.stories = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onSearch(term: string): void {
    this.page = 1;
    this.searchTerm = term;
    this.loadStories();
  }

  nextPage(): void {
    this.page++;
    this.loadStories();
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadStories();
    }
  }
}
