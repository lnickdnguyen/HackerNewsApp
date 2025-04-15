namespace HackerNewsApp.Services
{
    using HackerNewsApp.Contracts;

    public interface IHackerNewsService
    {
        Task<List<NewsStory>> GetNewestStoriesAsync(int page, int pageSize, string searchTerm = null);
    }
}