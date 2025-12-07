namespace GithubService
{
    using Octokit;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System;
    using System.Threading; 

    // ודאי ש-PortfolioRepositoryDto נמצא כאן או הוסף using מתאים אם הוא במקום אחר

    public class GitHubService : IGitHubService
    {
        // הלקוח נוצר בתוך השירות באמצעות הטוקן המוזרק
        private readonly GitHubClient _client;
        private readonly GitHubOptions _options;

        // *** תיקון קריטי: הקונסטרקטור צריך לקבל IOptions<GitHubOptions> ***
        public GitHubService(IOptions<GitHubOptions> options)
        {
            if (options == null) 
                throw new ArgumentNullException(nameof(options));

            // שמירת האפשרויות, כולל שם המשתמש והטוקן
            _options = options.Value ?? new GitHubOptions();

            // יצירת הלקוח (ללא credentials ברירת מחדל)
            var productHeader = new ProductHeaderValue("GithubPortfolioApp");
            _client = new GitHubClient(productHeader);

            // הוספת credentials רק אם קיים טוקן תקין
            if (!string.IsNullOrWhiteSpace(_options.Token))
            {
                _client.Credentials = new Credentials(_options.Token);
            }
        }

        public async Task<DateTimeOffset?> GetLastUserActivityTimeAsync()
        {
            // ודא ששם המשתמש קיים
            if (string.IsNullOrWhiteSpace(_options.UserName))
            {
                return null;
            }

            try
            {
                var request = new ApiOptions { PageSize = 1, PageCount = 1 };

                var events = await _client.Activity.Events.GetAllUserPerformed(_options.UserName, request);

                var latestEvent = events.FirstOrDefault();

                return latestEvent?.CreatedAt;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<PortfolioRepositoryDto>> GetPortfolioAsync()
        {
            var repos = await _client.Repository.GetAllForUser(_options.UserName);

            var semaphore = new SemaphoreSlim(5);

            var tasks = repos.Select(async r =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var dto = new PortfolioRepositoryDto
                    {
                        Name = r.Name,
                        FullName = r.FullName,
                        Description = r.Description,
                        Stars = r.StargazersCount,
                        HtmlUrl = r.HtmlUrl
                    };

                    //  שפות
                    try
                    {
                        var langs = await _client.Repository.GetAllLanguages(r.Owner.Login, r.Name);
                        // Convert to IDictionary<string, int> using Name and NumberOfBytes (נכון)
                        dto.Languages = langs.ToDictionary(lang => lang.Name, lang => (int)lang.NumberOfBytes);
                    }
                    catch
                    {
                        dto.Languages = new Dictionary<string, int>();
                    }

                    // קומיט אחרון
                    try
                    {
                        // שליפת הקומיט האחרון בלבד (pageSize=1)
                        var request = new ApiOptions { PageSize = 1, PageCount = 1 };
                        var commits = await _client.Repository.Commit.GetAll(r.Owner.Login, r.Name, request);
                        var latest = commits.FirstOrDefault();
                        // ודא שאתה ניגש ל-Author.Date בתוך ה-Commit (כפי שמופיע בקודך)
                        dto.LastCommitDate = latest?.Commit?.Author?.Date;
                    }
                    catch
                    {
                        dto.LastCommitDate = null;
                    }

                    //  מספר Pull Requests
                    try
                    {
                        // ניתן להשתמש ב-ApiOptions כדי להגביל את המידע שחוזר, אך הדרך הנוכחית תקינה
                        var prs = await _client.PullRequest.GetAllForRepository(r.Owner.Login, r.Name);
                        dto.PullRequestsCount = prs?.Count ?? 0;
                    }
                    catch
                    {
                        dto.PullRequestsCount = 0;
                    }

                    return dto;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var resultArray = await Task.WhenAll(tasks);

            //  מיון תוצאת החזרתית לפי כוכבים
            return resultArray.OrderByDescending(x => x.Stars);
        }

        public async Task<IReadOnlyList<Repository>> SearchRepositoriesAsync(string repoName, string language, string user)
        {
            var request = new SearchRepositoriesRequest(repoName)
            {
                User = user
            };

            if (!string.IsNullOrEmpty(language) && Enum.TryParse(language, true, out Language langEnum))
            {
                request.Language = langEnum;
            }

            var result = await _client.Search.SearchRepo(request);
            return result.Items;
        }
    }
}