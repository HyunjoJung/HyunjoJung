using Portfolio.Models;
using System.Text;
using System.Security.Claims;

namespace Portfolio.Services;

public class TerminalCommandService
{
    private readonly BlogService _blogService;
    private readonly CommentService _commentService;
    private readonly GitHubService _gitHubService;
    private readonly ILogger<TerminalCommandService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TerminalCommandService(
        BlogService blogService,
        CommentService commentService,
        GitHubService gitHubService,
        ILogger<TerminalCommandService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _blogService = blogService;
        _commentService = commentService;
        _gitHubService = gitHubService;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public async Task<CommandResult> ExecuteCommandAsync(string input, string currentPath)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new CommandResult { Success = true, Output = string.Empty };
        }

        var parts = ParseCommand(input);
        var command = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        return command switch
        {
            "help" => ShowHelp(),
            "clear" or "cls" => new CommandResult { Success = true, ClearScreen = true },
            "ls" => await ListDirectory(currentPath, args),
            "cd" => ChangeDirectory(currentPath, args),
            "pwd" => ShowCurrentPath(currentPath),
            "cat" => await ShowPost(args),
            "whoami" => ShowCurrentUser(),
            "login" => LoginCommand(args),
            "logout" => LogoutCommand(),
            "grep" => await SearchPosts(args),
            "comment" => await AddCommentAsync(args, currentPath),
            "issue" => await ShowIssuesAsync(args),
            "star" => await StarRepositoryAsync(),
            "discuss" => DiscussCommand(currentPath),
            "sudo" => new CommandResult { Success = false, Output = "Permission denied. Nice try though! 😏" },
            "exit" or "quit" => new CommandResult { Success = true, Output = "To exit, close your browser tab 😊", NavigateTo = "/" },
            _ => new CommandResult { Success = false, Output = $"bash: {command}: command not found\nType 'help' for available commands." }
        };
    }

    private string[] ParseCommand(string input)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        foreach (char c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (current.Length > 0)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            parts.Add(current.ToString());
        }

        return parts.ToArray();
    }

    private CommandResult ShowHelp()
    {
        var help = @"Available commands:
─────────────────────────────────────────────────────────
  Navigation:
    cd [path]         - Navigate to a page (blog, about, /)
    cd ..             - Go to parent directory
    cd ../about       - Relative path navigation
    cd ../../about    - Multi-level relative paths
    ls [path]         - List posts or comments
    pwd               - Show current location
    cat <post-slug>   - Read a blog post

  Search & Filter:
    grep <query>      - Search blog posts

  Authentication:
    login github      - Login with GitHub OAuth
    logout            - Logout from current session
    whoami            - Show current user info

  Comments:
    comment ""text""    - Post a comment (requires login)

  GitHub:
    issue [number]    - List open issues or view specific issue
    star              - Star this repository
    discuss           - Open GitHub Discussions (post-specific in blog)

  System:
    help              - Show this message
    clear / cls       - Clear terminal screen
    exit / quit       - Return to homepage
─────────────────────────────────────────────────────────
Examples:
  $ cd blog
  $ ls
  $ cat my-first-post
  $ cd ..             # Go back to blog
  $ cd ../about       # From blog to about
  $ cd ../blog        # From about to blog
  $ cd ../../about    # From post to about
  $ grep ""blazor""
  $ login github
  $ comment ""Great article!""
";
        return new CommandResult { Success = true, Output = help };
    }

    private async Task<CommandResult> ListDirectory(string currentPath, string[] args)
    {
        // Check for -al flag
        bool showAll = args.Any(a => a == "-al" || a == "-a" || a == "-l");
        var targetPath = args.FirstOrDefault(a => !a.StartsWith("-")) ?? currentPath;

        if (targetPath == "/" || targetPath == "~")
        {
            if (showAll)
            {
                return new CommandResult
                {
                    Success = true,
                    Output = @"total 12
drwxr-xr-x  3 hyunjo dev 4096 Dec  3 09:00 .
drwxr-xr-x 18 root   root 4096 Dec  2 10:00 ..
drwxr-xr-x  2 hyunjo dev 4096 Dec  2 10:30 blog/
-rw-r--r--  1 hyunjo dev 1234 Dec  2 10:30 about.md
-rw-r--r--  1 hyunjo dev  567 Dec  2 10:30 README.md"
                };
            }
            return new CommandResult
            {
                Success = true,
                Output = @"total 3
drwxr-xr-x  2 hyunjo dev 4096 Dec  2 10:30 blog/
-rw-r--r--  1 hyunjo dev 1234 Dec  2 10:30 about.md
-rw-r--r--  1 hyunjo dev  567 Dec  2 10:30 README.md"
            };
        }

        if (targetPath.Contains("blog"))
        {
            var posts = await _blogService.GetAllPostsAsync();
            var output = new StringBuilder();

            if (showAll)
            {
                output.AppendLine($"total {posts.Count + 2}");
                output.AppendLine("drwxr-xr-x  2 hyunjo dev 4096 Dec  3 09:00 .");
                output.AppendLine("drwxr-xr-x  3 hyunjo dev 4096 Dec  2 10:00 ..");
            }
            else
            {
                output.AppendLine($"total {posts.Count}");
            }

            foreach (var post in posts.Take(20))
            {
                var permissions = "-rw-r--r--";
                var size = post.ReadTimeMinutes * 200; // Approximate words
                var date = post.PublishedDate.ToString("MMM dd HH:mm");
                var tags = string.Join(",", post.Tags.Take(2));

                output.AppendLine($"{permissions}  1 hyunjo dev {size,5} {date} {post.Slug}.md  # {tags}");
            }

            if (posts.Count > 20)
            {
                output.AppendLine($"... and {posts.Count - 20} more posts");
            }

            return new CommandResult { Success = true, Output = output.ToString().TrimEnd() };
        }

        return new CommandResult { Success = false, Output = $"ls: cannot access '{targetPath}': No such file or directory" };
    }

    private CommandResult ChangeDirectory(string currentPath, string[] args)
    {
        if (args.Length == 0)
        {
            return new CommandResult { Success = true, NavigateTo = "/" };
        }

        var target = args[0].ToLower();

        // Handle relative paths with ..
        if (target.Contains(".."))
        {
            // Count how many levels to go up
            var parts = target.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var upLevels = parts.Count(p => p == "..");
            var destination = parts.LastOrDefault(p => p != "..");

            // Calculate the path after going up
            var pathParts = currentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

            // Remove levels from the end
            for (int i = 0; i < upLevels && pathParts.Count > 0; i++)
            {
                pathParts.RemoveAt(pathParts.Count - 1);
            }

            // If there's a destination after .., add it
            if (!string.IsNullOrEmpty(destination))
            {
                pathParts.Add(destination);
            }

            // Build the final path
            var finalPath = pathParts.Count == 0 ? "/" : "/" + string.Join("/", pathParts);

            // Validate the path exists
            if (finalPath == "/" || finalPath == "/blog" || finalPath == "/about")
            {
                return new CommandResult { Success = true, NavigateTo = finalPath };
            }

            return new CommandResult { Success = false, Output = $"bash: cd: {target}: No such file or directory" };
        }

        // Handle cd ..
        if (target == "..")
        {
            // Parse parent directory from current path
            // /blog/post-slug -> /blog
            // /blog -> /
            // / -> / (stay at root)
            if (currentPath == "/" || currentPath == "~")
            {
                return new CommandResult { Success = true, Output = "Already at root directory" };
            }

            if (currentPath.StartsWith("/blog/"))
            {
                return new CommandResult { Success = true, NavigateTo = "/blog" };
            }

            // /blog, /about, etc. -> /
            return new CommandResult { Success = true, NavigateTo = "/" };
        }

        return target switch
        {
            "~" or "/" or "home" => new CommandResult { Success = true, NavigateTo = "/" },
            "blog" or "blog/" => new CommandResult { Success = true, NavigateTo = "/blog" },
            "about" or "about/" => new CommandResult { Success = true, NavigateTo = "/about" },
            _ => new CommandResult { Success = false, Output = $"bash: cd: {target}: No such file or directory" }
        };
    }

    private CommandResult ShowCurrentPath(string currentPath)
    {
        return new CommandResult { Success = true, Output = currentPath };
    }

    private async Task<CommandResult> ShowPost(string[] args)
    {
        if (args.Length == 0)
        {
            return new CommandResult { Success = false, Output = "cat: missing file operand\nTry 'cat <post-slug>'" };
        }

        var slug = args[0].Replace(".md", "");
        var post = await _blogService.GetPostBySlugAsync(slug);

        if (post == null)
        {
            return new CommandResult { Success = false, Output = $"cat: {slug}: No such file or directory" };
        }

        return new CommandResult { Success = true, NavigateTo = $"/blog/{slug}" };
    }

    private CommandResult ShowCurrentUser()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity.Name ?? "unknown";
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var output = $"{username}@github (authenticated)";
            if (!string.IsNullOrEmpty(email))
            {
                output += $"\nEmail: {email}";
            }
            output += "\nType 'logout' to sign out";

            return new CommandResult
            {
                Success = true,
                Output = output
            };
        }

        return new CommandResult
        {
            Success = true,
            Output = "anonymous@guest (not authenticated)\nType 'login github' to authenticate"
        };
    }

    private CommandResult LoginCommand(string[] args)
    {
        if (args.Length == 0 || args[0].ToLower() != "github")
        {
            return new CommandResult { Success = false, Output = "Usage: login github" };
        }

        if (User?.Identity?.IsAuthenticated == true)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity.Name;
            return new CommandResult
            {
                Success = true,
                Output = $"Already logged in as {username}@github\nType 'logout' to sign out first"
            };
        }

        // Get the current host from HttpContext
        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request != null
            ? $"{request.Scheme}://{request.Host}"
            : "http://localhost:5050";

        return new CommandResult
        {
            Success = true,
            Output = $@"GitHub OAuth Login Required
─────────────────────────────────────────────────────────
To authenticate, please visit:

  {baseUrl}/login

This will redirect you to GitHub for authentication.
After authorizing, you'll be redirected back here.
─────────────────────────────────────────────────────────"
        };
    }

    private CommandResult LogoutCommand()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return new CommandResult
            {
                Success = true,
                Output = "You are not logged in.\nType 'login github' to authenticate"
            };
        }

        // Get the current host from HttpContext
        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request != null
            ? $"{request.Scheme}://{request.Host}"
            : "http://localhost:5050";

        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity.Name;

        return new CommandResult
        {
            Success = true,
            Output = $@"Logout
─────────────────────────────────────────────────────────
Currently logged in as: {username}@github

To logout, please visit:

  {baseUrl}/logout

You will be logged out and redirected to the homepage.
─────────────────────────────────────────────────────────"
        };
    }

    private async Task<CommandResult> SearchPosts(string[] args)
    {
        if (args.Length == 0)
        {
            return new CommandResult { Success = false, Output = "grep: missing search pattern" };
        }

        var query = string.Join(" ", args).Replace("\"", "");
        var posts = await _blogService.GetAllPostsAsync();
        var matches = posts.Where(p =>
            p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase))
        ).ToList();

        if (!matches.Any())
        {
            return new CommandResult { Success = true, Output = $"grep: no matches found for '{query}'" };
        }

        var output = new StringBuilder();
        output.AppendLine($"Found {matches.Count} match(es) for '{query}':");
        output.AppendLine("─────────────────────────────────────────────────────────");

        foreach (var post in matches.Take(10))
        {
            output.AppendLine($"{post.Slug}.md: {post.Title}");
            output.AppendLine($"  {post.Description}");
            output.AppendLine($"  Tags: {string.Join(", ", post.Tags)}");
            output.AppendLine();
        }

        if (matches.Count > 10)
        {
            output.AppendLine($"... and {matches.Count - 10} more results");
        }

        return new CommandResult { Success = true, Output = output.ToString().TrimEnd() };
    }

    private async Task<CommandResult> AddCommentAsync(string[] args, string currentPath)
    {
        if (args.Length == 0)
        {
            return new CommandResult { Success = false, Output = "Usage: comment \"your comment here\"" };
        }

        // Check if user is authenticated
        if (User?.Identity?.IsAuthenticated != true)
        {
            return new CommandResult
            {
                Success = false,
                Output = "Error: Authentication required to post comments.\nType 'login github' to authenticate."
            };
        }

        // Extract post slug from current path
        string? postSlug = null;
        if (currentPath.StartsWith("/blog/"))
        {
            postSlug = currentPath.Replace("/blog/", "").Trim('/');
        }

        if (string.IsNullOrEmpty(postSlug))
        {
            return new CommandResult
            {
                Success = false,
                Output = "Error: You must be viewing a blog post to comment.\nUse 'cd blog' and 'cat <post-slug>' to navigate to a post."
            };
        }

        // Verify post exists
        var post = await _blogService.GetPostBySlugAsync(postSlug);
        if (post == null)
        {
            return new CommandResult
            {
                Success = false,
                Output = $"Error: Post '{postSlug}' not found."
            };
        }

        // Get user information
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity.Name ?? "anonymous";
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var githubUsername = User.FindFirst("urn:github:login")?.Value ?? username;

        // Add the comment
        var content = string.Join(" ", args);
        var comment = await _commentService.AddCommentAsync(postSlug, username, email, githubUsername, content);

        var commentCount = _commentService.GetCommentCount(postSlug);
        return new CommandResult
        {
            Success = true,
            Output = $"Comment posted successfully!\n" +
                     $"By: {username}@github\n" +
                     $"On: {post.Title}\n" +
                     $"Total comments: {commentCount}"
        };
    }

    private async Task<CommandResult> ShowIssuesAsync(string[] args)
    {
        try
        {
            // If a specific issue number is provided
            if (args.Length > 0 && int.TryParse(args[0], out int issueNumber))
            {
                var issue = await _gitHubService.GetIssueAsync(issueNumber);
                if (issue == null)
                {
                    return new CommandResult
                    {
                        Success = false,
                        Output = $"Issue #{issueNumber} not found."
                    };
                }

                var output = new StringBuilder();
                output.AppendLine($"Issue #{issue.Number}: {issue.Title}");
                output.AppendLine($"State: {issue.State}");
                output.AppendLine($"Author: {issue.User.Login}");
                output.AppendLine($"Created: {issue.CreatedAt:yyyy-MM-dd}");
                output.AppendLine($"Comments: {issue.Comments}");
                output.AppendLine($"\n{issue.Body}");
                output.AppendLine($"\nURL: {issue.HtmlUrl}");

                return new CommandResult { Success = true, Output = output.ToString() };
            }

            // List all open issues
            var issues = await _gitHubService.GetOpenIssuesAsync();
            if (!issues.Any())
            {
                return new CommandResult
                {
                    Success = true,
                    Output = "No open issues found.\nCreate a new issue at: https://github.com/HyunjoJung/HyunjoJung/issues"
                };
            }

            var listOutput = new StringBuilder();
            listOutput.AppendLine($"Open Issues ({issues.Count}):");
            listOutput.AppendLine("─────────────────────────────────────────────────────────");

            foreach (var issue in issues.Take(10))
            {
                listOutput.AppendLine($"#{issue.Number} - {issue.Title}");
                listOutput.AppendLine($"  by {issue.User.Login} | {issue.Comments} comments");
                listOutput.AppendLine();
            }

            if (issues.Count > 10)
            {
                listOutput.AppendLine($"... and {issues.Count - 10} more issues");
            }

            listOutput.AppendLine("Use 'issue <number>' to view details");

            return new CommandResult { Success = true, Output = listOutput.ToString().TrimEnd() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch GitHub issues");
            return new CommandResult
            {
                Success = false,
                Output = "Failed to fetch GitHub issues. Please try again later."
            };
        }
    }

    private async Task<CommandResult> StarRepositoryAsync()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            // Show star count for unauthenticated users
            var starCount = await _gitHubService.GetStarCountAsync();
            return new CommandResult
            {
                Success = true,
                Output = $"⭐ This repository has {starCount} stars!\n\n" +
                         $"Login to star: login github\n" +
                         $"Repository: https://github.com/HyunjoJung/HyunjoJung"
            };
        }

        try
        {
            // Check if already starred
            var isStarred = await _gitHubService.CheckStarredAsync();
            if (isStarred)
            {
                var starCount = await _gitHubService.GetStarCountAsync();
                return new CommandResult
                {
                    Success = true,
                    Output = $"⭐ You've already starred this repository!\n" +
                             $"Total stars: {starCount}\n" +
                             $"Repository: https://github.com/HyunjoJung/HyunjoJung"
                };
            }

            // Star the repository
            var success = await _gitHubService.StarRepositoryAsync();
            if (success)
            {
                var starCount = await _gitHubService.GetStarCountAsync();
                return new CommandResult
                {
                    Success = true,
                    Output = $"⭐ Thank you for starring the repository!\n" +
                             $"Total stars: {starCount}\n" +
                             $"Repository: https://github.com/HyunjoJung/HyunjoJung"
                };
            }

            return new CommandResult
            {
                Success = false,
                Output = "Failed to star repository. Please try again later."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to star repository");
            return new CommandResult
            {
                Success = false,
                Output = "Failed to star repository. Please try again later."
            };
        }
    }

    private CommandResult DiscussCommand(string currentPath)
    {
        // Check if viewing a blog post
        if (currentPath.StartsWith("/blog/"))
        {
            var postSlug = currentPath.Replace("/blog/", "").Trim('/');

            if (!string.IsNullOrEmpty(postSlug))
            {
                // Redirect to GitHub discussions for this specific post
                return new CommandResult
                {
                    Success = true,
                    Output = $"Opening discussions for this post...\nPost: {postSlug}",
                    NavigateTo = $"https://github.com/HyunjoJung/HyunjoJung/discussions?discussions_q={Uri.EscapeDataString(postSlug)}"
                };
            }
        }

        // General discussions page
        return new CommandResult
        {
            Success = true,
            Output = "Opening GitHub Discussions...",
            NavigateTo = "https://github.com/HyunjoJung/HyunjoJung/discussions"
        };
    }
}
