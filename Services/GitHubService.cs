using Octokit;

namespace Portfolio.Services;

public class GitHubService
{
    private readonly GitHubClient _client;
    private readonly string _owner;
    private readonly string _repo;
    private readonly ILogger<GitHubService> _logger;

    public GitHubService(IConfiguration configuration, ILogger<GitHubService> logger)
    {
        _logger = logger;
        _owner = configuration["GitHub:Owner"] ?? "HyunjoJung";
        _repo = configuration["GitHub:Repo"] ?? "HyunjoJung";

        _client = new GitHubClient(new ProductHeaderValue("Portfolio-Blog"));

        // Optional: Add token if provided for higher rate limits
        var token = configuration["GitHub:Token"];
        if (!string.IsNullOrEmpty(token))
        {
            _client.Credentials = new Credentials(token);
        }
    }

    public async Task<List<Issue>> GetOpenIssuesAsync()
    {
        try
        {
            var issues = await _client.Issue.GetAllForRepository(_owner, _repo, new RepositoryIssueRequest
            {
                State = ItemStateFilter.Open
            });

            return issues.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch GitHub issues");
            return new List<Issue>();
        }
    }

    public async Task<Issue?> GetIssueAsync(int issueNumber)
    {
        try
        {
            return await _client.Issue.Get(_owner, _repo, issueNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch GitHub issue #{IssueNumber}", issueNumber);
            return null;
        }
    }

    public async Task<bool> StarRepositoryAsync()
    {
        try
        {
            return await _client.Activity.Starring.StarRepo(_owner, _repo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to star repository");
            return false;
        }
    }

    public async Task<bool> CheckStarredAsync()
    {
        try
        {
            return await _client.Activity.Starring.CheckStarred(_owner, _repo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if repository is starred");
            return false;
        }
    }

    public async Task<int> GetStarCountAsync()
    {
        try
        {
            var repo = await _client.Repository.Get(_owner, _repo);
            return repo.StargazersCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get star count");
            return 0;
        }
    }
}
