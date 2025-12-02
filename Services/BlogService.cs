using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Portfolio.Models;
using System.IO;
using YamlDotNet.Serialization;

namespace Portfolio.Services;

public class BlogService : IDisposable
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<BlogService> _logger;
    private readonly MarkdownPipeline _pipeline;
    private List<BlogPost>? _cachedPosts;
    private FileSystemWatcher? _fileWatcher;
    private Timer? _debounceTimer;
    private readonly object _debounceLock = new();

    public BlogService(IWebHostEnvironment env, ILogger<BlogService> logger)
    {
        _env = env;
        _logger = logger;
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter() // Enable YAML front matter parsing
            .Build();

        // Enable file watching in Development environment only
        if (_env.IsDevelopment())
        {
            SetupFileWatcher();
        }
    }

    private void SetupFileWatcher()
    {
        var postsPath = Path.Combine(_env.ContentRootPath, "Posts");

        if (!Directory.Exists(postsPath))
        {
            _logger.LogWarning("Cannot setup file watcher: Posts directory not found at {PostsPath}", postsPath);
            return;
        }

        _fileWatcher = new FileSystemWatcher(postsPath, "*.md")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };

        _fileWatcher.Created += OnPostFileChanged;
        _fileWatcher.Changed += OnPostFileChanged;
        _fileWatcher.Deleted += OnPostFileChanged;
        _fileWatcher.Renamed += OnPostFileChanged;

        _logger.LogInformation("File watcher enabled for {PostsPath} (Development mode)", postsPath);
    }

    private void OnPostFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("Detected file change: {ChangeType} - {FileName}. Debouncing cache clear...", e.ChangeType, Path.GetFileName(e.FullPath));

        // Debounce: delay cache clear by 500ms, restart timer on each event
        lock (_debounceLock)
        {
            _debounceTimer?.Change(Timeout.Infinite, 0); // Cancel pending timer
            _debounceTimer ??= new Timer(_ => DebouncedCacheClear(), null, Timeout.Infinite, Timeout.Infinite);
            _debounceTimer.Change(500, Timeout.Infinite); // Trigger after 500ms of inactivity
        }
    }

    private void DebouncedCacheClear()
    {
        _logger.LogInformation("File changes detected. Clearing blog cache after debounce delay.");
        ClearCache();
    }
    
    // A private helper class for deserializing the YAML front matter
    private class FrontMatter
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; } = string.Empty;

        [YamlMember(Alias = "description")]
        public string Description { get; set; } = string.Empty;

        [YamlMember(Alias = "date")]
        public DateTime PublishedDate { get; set; }

        [YamlMember(Alias = "tags")]
        public List<string> Tags { get; set; } = new();

        [YamlMember(Alias = "category")]
        public string Category { get; set; } = string.Empty;

        [YamlMember(Alias = "featured")]
        public bool IsFeatured { get; set; }

        [YamlMember(Alias = "image")]
        public string? ImageUrl { get; set; }
    }

    public async Task<List<BlogPost>> GetAllPostsAsync()
    {
        if (_cachedPosts != null)
            return _cachedPosts;

        var posts = new List<BlogPost>();
        var postsPath = Path.Combine(_env.ContentRootPath, "Posts");

        if (!Directory.Exists(postsPath))
        {
            _logger.LogWarning("Posts directory not found at {PostsPath}", postsPath);
            return posts;
        }

        var files = Directory.GetFiles(postsPath, "*.md");

        foreach (var file in files)
        {
            var post = await ParseMarkdownFileAsync(file);
            if (post != null)
                posts.Add(post);
        }

        _cachedPosts = posts.OrderByDescending(p => p.PublishedDate).ToList();
        _logger.LogInformation("Successfully loaded and cached {PostCount} blog posts.", _cachedPosts.Count);
        return _cachedPosts;
    }

    public async Task<BlogPost?> GetPostBySlugAsync(string slug)
    {
        var posts = await GetAllPostsAsync();
        return posts.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<BlogPost>> GetPostsByTagAsync(string tag)
    {
        var posts = await GetAllPostsAsync();
        return posts.Where(p => p.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                   .ToList();
    }

    public async Task<List<BlogPost>> GetPostsByCategoryAsync(string category)
    {
        var posts = await GetAllPostsAsync();
        return posts.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                   .ToList();
    }

    public async Task<List<BlogPost>> GetFeaturedPostsAsync()
    {
        var posts = await GetAllPostsAsync();
        return posts.Where(p => p.IsFeatured).ToList();
    }

    public async Task<List<string>> GetAllTagsAsync()
    {
        var posts = await GetAllPostsAsync();
        return posts.SelectMany(p => p.Tags)
                   .Distinct()
                   .OrderBy(t => t)
                   .ToList();
    }

    public async Task<List<string>> GetAllCategoriesAsync()
    {
        var posts = await GetAllPostsAsync();
        return posts.Select(p => p.Category)
                   .Distinct()
                   .Where(c => !string.IsNullOrEmpty(c))
                   .OrderBy(c => c)
                   .ToList();
    }

    private async Task<BlogPost?> ParseMarkdownFileAsync(string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var content = await File.ReadAllTextAsync(filePath);

            var document = Markdown.Parse(content, _pipeline);

            var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

            if (yamlBlock == null)
            {
                _logger.LogWarning("Skipping {FileName}: No YAML front matter found.", fileName);
                return null;
            }

            var deserializer = new DeserializerBuilder().Build();
            var frontMatter = deserializer.Deserialize<FrontMatter>(yamlBlock.Lines.ToString());

            if (frontMatter == null)
            {
                _logger.LogWarning("Skipping {FileName}: Failed to deserialize YAML front matter.", fileName);
                return null;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(frontMatter.Title))
            {
                _logger.LogWarning("Validation warning for {FileName}: Missing 'title' field. Using filename as fallback.", fileName);
                frontMatter.Title = Path.GetFileNameWithoutExtension(filePath);
            }

            if (frontMatter.PublishedDate == default(DateTime))
            {
                _logger.LogWarning("Validation warning for {FileName}: Missing or invalid 'date' field. Using file creation date as fallback.", fileName);
                frontMatter.PublishedDate = File.GetCreationTime(filePath);
            }

            if (string.IsNullOrWhiteSpace(frontMatter.Description))
            {
                _logger.LogWarning("Validation warning for {FileName}: Missing 'description' field. SEO may be affected.", fileName);
                frontMatter.Description = string.Empty;
            }
            
            using var writer = new StringWriter();
            var renderer = new Markdig.Renderers.HtmlRenderer(writer);
            _pipeline.Setup(renderer);

            foreach (var block in document)
            {
                if (block is not YamlFrontMatterBlock)
                {
                    renderer.Render(block);
                }
            }
            writer.Flush();
            var htmlContent = writer.ToString();

            var post = new BlogPost
            {
                Slug = Path.GetFileNameWithoutExtension(filePath),
                Title = frontMatter.Title,
                Description = frontMatter.Description,
                PublishedDate = frontMatter.PublishedDate,
                Tags = frontMatter.Tags,
                Category = frontMatter.Category,
                IsFeatured = frontMatter.IsFeatured,
                ImageUrl = frontMatter.ImageUrl,
                Content = htmlContent
            };

            var plainText = Markdown.ToPlainText(content);
            var wordCount = plainText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            post.ReadTimeMinutes = Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));

            return post;
        }
        catch (Exception ex)
        {
            var fileName = Path.GetFileName(filePath);
            _logger.LogError(ex,
                "Failed to parse markdown file {FileName}. Error: {ErrorType}, Message: {ErrorMessage}",
                fileName, ex.GetType().Name, ex.Message);
            return null;
        }
    }

    public void ClearCache()
    {
        _cachedPosts = null;
        _logger.LogInformation("Blog post cache has been cleared.");
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
        _fileWatcher?.Dispose();
    }
}
