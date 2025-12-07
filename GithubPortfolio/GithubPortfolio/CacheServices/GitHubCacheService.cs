using GithubPortfolio.CacheServices;
using GithubService;
using Microsoft.Extensions.Caching.Memory;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GithubService.CacheServices
{
    public class GitHubCacheService : IGitHubService
    {
        private readonly IGitHubService _innerService;
        private readonly IMemoryCache _cacheService;

        private const string PortfolioCacheKey = "PortfolioCacheKey";
        private const string SearchCacheKeyPrefix = "SearchCacheKey";
        
        public GitHubCacheService(IGitHubService innerService, IMemoryCache cacheService)
        {
            _innerService = innerService;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<PortfolioRepositoryDto>> GetPortfolioAsync()
        {
            if (_cacheService.TryGetValue(PortfolioCacheKey, out CachedPortfolio cachedData) && cachedData != null)
            {
                var lastActivityTime = await _innerService.GetLastUserActivityTimeAsync();

                if (lastActivityTime == null || cachedData.LastFetchedTime >= lastActivityTime)
                {
                    return cachedData.Repositories;
                }

            }

            var portfolio = await _innerService.GetPortfolioAsync();

            var freshData = new CachedPortfolio
            {
                Repositories = portfolio,
                LastFetchedTime = DateTimeOffset.UtcNow
            };


            _cacheService.Set(PortfolioCacheKey, freshData, TimeSpan.FromDays(7));

            return portfolio;
        }

        public Task<DateTimeOffset?> GetLastUserActivityTimeAsync()
        {
            return _innerService.GetLastUserActivityTimeAsync();
        }

        public async Task<IReadOnlyList<Repository>> SearchRepositoriesAsync(string repoName, string language, string user)
        {
            var key = $"{SearchCacheKeyPrefix}_{repoName ?? string.Empty}_{language ?? string.Empty}_{user ?? string.Empty}";

            if (_cacheService.TryGetValue(key, out IReadOnlyList<Repository> cachedResult))
                return cachedResult;

            var result = await _innerService.SearchRepositoriesAsync(repoName, language, user);

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            _cacheService.Set(key, result, cacheEntryOptions);

            return result;
        }
    }
}
