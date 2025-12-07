using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GithubService
{

    public class PortfolioRepositoryDto
    {
        public string Name { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Description { get; set; }
        public IDictionary<string, int> Languages { get; set; } = new Dictionary<string, int>();
        public DateTimeOffset? LastCommitDate { get; set; }
        public int Stars { get; set; }
        public int PullRequestsCount { get; set; }
        public string? HtmlUrl { get; set; }
    }
}
