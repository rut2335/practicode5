using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;


namespace GithubService
{
    public interface IGitHubService
    {
        Task<IEnumerable<PortfolioRepositoryDto>> GetPortfolioAsync();
        Task<DateTimeOffset?> GetLastUserActivityTimeAsync();
        Task<IReadOnlyList<Repository>> SearchRepositoriesAsync(string repoName, string language, string user);

    }
}
