using Portfolio.Components;
using Portfolio.Services;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNet.Security.OAuth.GitHub;

var builder = WebApplication.CreateBuilder(args);

// Ensure logs directory exists
var logsPath = Path.Combine(builder.Environment.ContentRootPath, "logs");
if (!Directory.Exists(logsPath))
{
    Directory.CreateDirectory(logsPath);
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(logsPath, "portfolio-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"] ?? "";
    options.Scope.Add("user:email");
    options.CallbackPath = "/signin-github";
});

// Add blog service
builder.Services.AddSingleton<BlogService>();
builder.Services.AddSingleton<ViewCountService>();
builder.Services.AddSingleton<CommentService>();
builder.Services.AddSingleton<GitHubService>();
builder.Services.AddScoped<TerminalCommandService>();
builder.Services.AddScoped<BrowserStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    // Content Security Policy (Blazor Server compatible)
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://pagead2.googlesyndication.com; " +
        "style-src 'self' 'unsafe-inline' blob: https://cdn.jsdelivr.net; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "connect-src 'self' ws: wss:; " +
        "frame-ancestors 'none';");

    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Auth endpoints
app.MapGet("/login", async (HttpContext context) =>
{
    await context.ChallengeAsync(GitHubAuthenticationDefaults.AuthenticationScheme,
        new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = "/"
        });
});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

// Sitemap endpoint
app.MapGet("/sitemap.xml", async (BlogService blogService) =>
{
    var baseUrl = "https://hyunjo.uk";
    var posts = await blogService.GetAllPostsAsync();
    var tags = await blogService.GetAllTagsAsync();

    var sitemap = new System.Text.StringBuilder();
    sitemap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    sitemap.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

    // Homepage / Blog
    sitemap.AppendLine("    <url>");
    sitemap.AppendLine($"        <loc>{baseUrl}/</loc>");
    sitemap.AppendLine($"        <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
    sitemap.AppendLine("        <changefreq>daily</changefreq>");
    sitemap.AppendLine("        <priority>1.0</priority>");
    sitemap.AppendLine("    </url>");

    // About page
    sitemap.AppendLine("    <url>");
    sitemap.AppendLine($"        <loc>{baseUrl}/about</loc>");
    sitemap.AppendLine($"        <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
    sitemap.AppendLine("        <changefreq>monthly</changefreq>");
    sitemap.AppendLine("        <priority>0.8</priority>");
    sitemap.AppendLine("    </url>");

    // Blog posts
    foreach (var post in posts)
    {
        sitemap.AppendLine("    <url>");
        sitemap.AppendLine($"        <loc>{baseUrl}/blog/{post.Slug}</loc>");
        sitemap.AppendLine($"        <lastmod>{post.PublishedDate:yyyy-MM-dd}</lastmod>");
        sitemap.AppendLine("        <changefreq>monthly</changefreq>");
        sitemap.AppendLine("        <priority>0.9</priority>");
        sitemap.AppendLine("    </url>");
    }

    // Tag pages
    foreach (var tag in tags)
    {
        sitemap.AppendLine("    <url>");
        sitemap.AppendLine($"        <loc>{baseUrl}/blog/tag/{tag}</loc>");
        sitemap.AppendLine($"        <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
        sitemap.AppendLine("        <changefreq>weekly</changefreq>");
        sitemap.AppendLine("        <priority>0.7</priority>");
        sitemap.AppendLine("    </url>");
    }

    sitemap.AppendLine("</urlset>");

    return Results.Content(sitemap.ToString(), "application/xml");
});

app.Run();
