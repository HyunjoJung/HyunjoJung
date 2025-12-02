namespace Portfolio.Models;

public class Comment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PostSlug { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GitHubUsername { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsAuthor { get; set; } = false;
}

public class CommentList
{
    public List<Comment> Comments { get; set; } = new();
}
