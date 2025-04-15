namespace HackerNewsApp.Services
{
    using HackerNewsApp.Contracts;
    using Microsoft.Extensions.Caching.Memory;

    public class HackerNewsService : IHackerNewsService
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://hacker-news.firebaseio.com/v0";
        private const string LatestCacheKey = "latest_stories";
        private readonly static TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

        public HackerNewsService(IMemoryCache cache, HttpClient httpClient)
        {
            _cache = cache;
            _httpClient = httpClient;
        }

        public async Task<List<NewsStory>> GetNewestStoriesAsync(int page, int pageSize, string searchTerm = null)
        {
            var cacheKey = $"{page}_{pageSize}_{searchTerm}";
            if (!_cache.TryGetValue(cacheKey, out List<NewsStory> stories))
            {
                var allStories = await GetCachedNewestStoriesAsync();

                stories = new List<NewsStory>(pageSize);
                var matchesToSkip = (page - 1) * pageSize;
                foreach (var story in allStories)
                {
                    if (story == null || (!string.IsNullOrWhiteSpace(searchTerm) &&
                        !story.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    if (matchesToSkip > 0)
                    {
                        matchesToSkip--;
                        continue;
                    }

                    stories.Add(story);

                    if (stories.Count >= pageSize)
                    {
                        break;
                    }
                }
            }
            
            _cache.Set(cacheKey, stories, CacheTtl);
            return stories;
        }

        private async Task<List<NewsStory>> GetCachedNewestStoriesAsync()
        {
            if (!_cache.TryGetValue(LatestCacheKey, out List<NewsStory> allStories))
            {
                var storyIds = await _httpClient.GetFromJsonAsync<List<int>>($"{BaseUrl}/newstories.json");

                var storyTasks = storyIds.Select(storyId => _httpClient.GetFromJsonAsync<NewsStory>($"{BaseUrl}/item/{storyId}.json")).ToList();
                await Task.WhenAll(storyTasks);

                allStories = storyTasks.Select(task => task.Result).Where(story => story != null).ToList();

                _cache.Set(LatestCacheKey, allStories, CacheTtl);
            }

            return allStories;
        }
    }
}