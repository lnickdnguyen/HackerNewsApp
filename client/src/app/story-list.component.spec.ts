import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StoryListComponent } from './story-list.component';
import { StoryService, Story } from './story-service';
import { of, throwError } from 'rxjs';
import { provideHttpClient } from '@angular/common/http';

describe('StoryListComponent', () => {
  let component: StoryListComponent;
  let fixture: ComponentFixture<StoryListComponent>;
  let storyServiceSpy: jasmine.SpyObj<StoryService>;

  const mockStories: Story[] = [
    { id: 1, title: 'Story 1', url: 'https://example.com/1' },
    { id: 2, title: 'Story 2', url: 'https://example.com/2' },
  ];

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('StoryService', ['getStories']);

    await TestBed.configureTestingModule({
      imports: [StoryListComponent],
      providers: [
        provideHttpClient(),
        { provide: StoryService, useValue: spy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(StoryListComponent);
    component = fixture.componentInstance;
    storyServiceSpy = TestBed.inject(StoryService) as jasmine.SpyObj<StoryService>;
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should load stories on init', () => {
    storyServiceSpy.getStories.and.returnValue(of(mockStories));
    fixture.detectChanges(); // triggers ngOnInit

    expect(storyServiceSpy.getStories).toHaveBeenCalledWith(1, 20, '');
    expect(component.stories.length).toBe(2);
  });

  it('should update stories on search', () => {
    storyServiceSpy.getStories.and.returnValue(of(mockStories));
    component.onSearch('angular');

    expect(component.page).toBe(1);
    expect(component.searchTerm).toBe('angular');
    expect(storyServiceSpy.getStories).toHaveBeenCalledWith(1, 20, 'angular');
  });

  it('should go to next page and load stories', () => {
    storyServiceSpy.getStories.and.returnValue(of(mockStories));
    component.page = 1;

    component.nextPage();

    expect(component.page).toBe(2);
    expect(storyServiceSpy.getStories).toHaveBeenCalledWith(2, 20, '');
  });

  it('should go to previous page if not on first', () => {
    storyServiceSpy.getStories.and.returnValue(of(mockStories));
    component.page = 2;

    component.prevPage();

    expect(component.page).toBe(1);
    expect(storyServiceSpy.getStories).toHaveBeenCalledWith(1, 20, '');
  });

  it('should not go to previous page if already on page 1', () => {
    component.page = 1;
    component.prevPage();

    expect(component.page).toBe(1);
    expect(storyServiceSpy.getStories).not.toHaveBeenCalled();
  });

  it('should handle errors when loading stories', () => {
    storyServiceSpy.getStories.and.returnValue(throwError(() => new Error('API failed')));
    component.loadStories();

    expect(component.loading).toBeFalse();
    expect(component.stories.length).toBe(0);
  });
});
