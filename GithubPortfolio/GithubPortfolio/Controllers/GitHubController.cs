using Microsoft.AspNetCore.Mvc;
using GithubService;

namespace GithubPortfolio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        private readonly IGitHubService _gitHubService;

        public GitHubController(IGitHubService gitHubService)
        {
            _gitHubService = gitHubService;
        }

        [HttpGet("portfolio")]
        public async Task<IActionResult> GetPortfolio()
        {
            var repos = await _gitHubService.GetPortfolioAsync();
            return Ok(repos);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string repoName, [FromQuery] string language, [FromQuery] string user)
        {
            var results = await _gitHubService.SearchRepositoriesAsync(repoName, language, user);
            return Ok(results);
        }
    }
}
