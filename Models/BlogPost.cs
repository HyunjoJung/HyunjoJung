namespace Portfolio.Models;

public class BlogPost
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = "Hyunjo Jung";
    public int ReadTimeMinutes { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public string? ImageUrl { get; set; }
}
