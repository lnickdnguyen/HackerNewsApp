namespace HackerNewsApp.Tests
{
    using HackerNewsApp.Contracts;
    using HackerNewsApp.Services;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    [TestClass]
    public class HackerNewsServiceTests
    {
        private readonly List<NewsStory> AllNewsStories = new List<NewsStory>
        {
            new NewsStory { Id = 1, Title = "Test Story 1" },
            new NewsStory { Id = 2, Title = "Test Story 2" },
            new NewsStory { Id = 3, Title = "Test Story 3" },
            new NewsStory { Id = 4, Title = "Test Story 4" },
            new NewsStory { Id = 5, Title = "Test Story 5" }
        };

        private readonly HackerNewsService _hackerNewsService;
        private readonly MemoryCache _memoryCache;
        private readonly Mock<HttpMessageHandler> _messageHandler;

        public HackerNewsServiceTests()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            var storyIds = AllNewsStories.Select(s => s.Id).ToList();
            _messageHandler = new Mock<HttpMessageHandler>();
            _messageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.ToString().EndsWith("/newstories.json")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(storyIds))
                });

            foreach (var id in storyIds)
            {
                _messageHandler
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.ToString().EndsWith($"/item/{id}.json")),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .ReturnsAsync(new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(AllNewsStories[id - 1]))
                    });
            }

            _hackerNewsService = new HackerNewsService(_memoryCache, new HttpClient(_messageHandler.Object));
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithCachedStoryIds_ReturnsExpectedStories()
        {
            // Arrange
            var page = 1;
            var pageSize = 2;
            _memoryCache.Set($"{page}_{pageSize}_", AllNewsStories.Take(2));

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize);

            // Assert
            Assert.AreEqual(pageSize, result.Count);
            Assert.AreEqual(AllNewsStories[0].Id, result[0].Id);
            Assert.AreEqual(AllNewsStories[1].Id, result[1].Id);
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithCachedStoryIds_ReturnsExpectedStories_NextPage()
        {
            // Arrange
            var page = 2;
            var pageSize = 2;
            _memoryCache.Set($"{page - 1}_{pageSize}_", AllNewsStories.Take(pageSize));
            _memoryCache.Set($"{page}_{pageSize}_", AllNewsStories.Skip(pageSize).Take(pageSize));

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize);

            // Assert
            Assert.AreEqual(pageSize, result.Count);
            Assert.AreEqual(AllNewsStories[2].Id, result[0].Id);
            Assert.AreEqual(AllNewsStories[3].Id, result[1].Id);
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithCachedStoryIds_ReturnsExpectedStories_LastPage()
        {
            // Arrange
            var page = 3;
            var pageSize = 2;
            _memoryCache.Set($"{page - 2}_{pageSize}_", AllNewsStories.Skip(2 * pageSize).Take(pageSize));
            _memoryCache.Set($"{page - 1}_{pageSize}_", AllNewsStories.Skip(pageSize).Take(pageSize));
            _memoryCache.Set($"{page}_{pageSize}_", AllNewsStories.Take(pageSize));

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(AllNewsStories[4].Id, result[0].Id);
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithUncachedStoryIds_ReturnsExpectedStories()
        {
            // Arrange
            var page = 1;
            var pageSize = 2;

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize);

            // Assert
            Assert.AreEqual(pageSize, result.Count);
            Assert.AreEqual(AllNewsStories[0].Id, result[0].Id);
            Assert.AreEqual(AllNewsStories[1].Id, result[1].Id);
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithUncachedStoryIds_ReturnsExpectedStories_NextPage()
        {
            // Arrange
            var page = 2;
            var pageSize = 2;

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize);

            // Assert
            Assert.AreEqual(pageSize, result.Count);
            Assert.AreEqual(AllNewsStories[2].Id, result[0].Id);
            Assert.AreEqual(AllNewsStories[3].Id, result[1].Id);
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithUncachedStoryIds_ReturnsExpectedStories_LastPage()
        {
            // Arrange
            var page = 3;
            var pageSize = 2;

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(AllNewsStories[4].Id, result[0].Id);
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithMatchingStories_ReturnsExpectedStories()
        {
            // Arrange
            var page = 1;
            var pageSize = 2;
            var searchTerm = "test";

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize, searchTerm);

            // Assert
            Assert.AreEqual(pageSize, result.Count);
            Assert.AreEqual(AllNewsStories[0].Id, result[0].Id);
            Assert.IsTrue(result[0].Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(AllNewsStories[1].Id, result[1].Id);
            Assert.IsTrue(result[1].Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithMatchingStories_ReturnsExpectedStories_NextPage()
        {
            // Arrange
            var page = 2;
            var pageSize = 2;
            var searchTerm = "test";

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize, searchTerm);

            // Assert
            Assert.AreEqual(pageSize, result.Count);
            Assert.AreEqual(AllNewsStories[2].Id, result[0].Id);
            Assert.IsTrue(result[0].Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(AllNewsStories[3].Id, result[1].Id);
            Assert.IsTrue(result[1].Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithMatchingStories_ReturnsExpectedStories_LastPage()
        {
            // Arrange
            var page = 3;
            var pageSize = 2;
            var searchTerm = "test";

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize, searchTerm);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(AllNewsStories[4].Id, result[0].Id);
            Assert.IsTrue(result[0].Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithNonMatchingStories_ReturnsEmptyList()
        {
            // Arrange
            var page = 1;
            var pageSize = 2;

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize, "non-matching");

            // Assert
            Assert.IsEmpty(result);
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithNonMatchingStories_ReturnsEmptyList_NextPage()
        {
            // Arrange
            var page = 2;
            var pageSize = 2;

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize, "non-matching");

            // Assert
            Assert.IsEmpty(result);
        }

        [TestMethod]
        public async Task GetNewestStoriesAsync_WithNonMatchingStories_ReturnsEmptyList_LastPage()
        {
            // Arrange
            var page = 3;
            var pageSize = 2;

            // Act
            var result = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize, "non-matching");

            // Assert
            Assert.IsEmpty(result);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _memoryCache.Clear();
        }
    }
}
