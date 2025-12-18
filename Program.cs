using Portfolio.Components;
using Portfolio.Services;
using Portfolio.Resources;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

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

// Add Localization
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("ko")
    };

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Priority: QueryString > Cookie > Accept-Language Header
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

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
builder.Services.AddScoped<LocaleService>();

var app = builder.Build();

// HEAD request support for SEO crawlers (MUST BE FIRST!)
app.Use(async (context, next) =>
{
    var originalMethod = context.Request.Method;
    if (HttpMethods.IsHead(context.Request.Method))
    {
        context.Request.Method = HttpMethods.Get;
    }

    await next();

    if (HttpMethods.IsHead(originalMethod))
    {
        context.Response.Body = Stream.Null;
    }
});

// Configure Forwarded Headers for proxy servers like Cloudflare/Nginx
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};
// Clear default restrictions to trust all proxies (required for Cloudflare Tunnel)
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Use Request Localization
app.UseRequestLocalization();

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
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://pagead2.googlesyndication.com https://googletagservices.com https://static.cloudflareinsights.com https://*.google.com https://*.google https://*.gstatic.com; " + // Blazor requires unsafe-eval, allow AdSense and Cloudflare
        "style-src 'self' 'unsafe-inline' blob: https://cdn.jsdelivr.net https://fonts.googleapis.com; " + // Allow Google Fonts
        "img-src 'self' data: https://pagead2.googlesyndication.com https://*.google.com https://*.google https://*.gstatic.com https://*.doubleclick.net; " + // Allow AdSense images
        "font-src 'self' https://cdn.jsdelivr.net https://fonts.gstatic.com; " + // Allow Google Fonts
        "frame-src https://googleads.g.doubleclick.net https://tpc.googlesyndication.com https://*.google.com https://*.google; " + // Allow AdSense iframes
        "connect-src 'self' ws: wss: https://pagead2.googlesyndication.com https://cloudflareinsights.com https://*.google.com https://*.google https://*.doubleclick.net; " + // WebSocket for Blazor SignalR, AdSense and Cloudflare connections
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'");

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

// robots.txt endpoint (supports both GET and HEAD)
app.MapMethods("/robots.txt", new[] { "GET", "HEAD" }, () =>
{
    var robotsTxt = @"User-agent: *
Allow: /

# AI Crawlers - explicitly allow for AEO (Answer Engine Optimization)
User-agent: ChatGPT-User
Allow: /

User-agent: GPTBot
Allow: /

User-agent: Claude-Web
Allow: /

User-agent: anthropic-ai
Allow: /

User-agent: PerplexityBot
Allow: /

User-agent: Applebot-Extended
Allow: /

Sitemap: https://hyunjo.uk/sitemap.xml";
    return Results.Text(robotsTxt, "text/plain");
}).DisableAntiforgery();

// Sitemap endpoint (supports both GET and HEAD)
app.MapMethods("/sitemap.xml", new[] { "GET", "HEAD" }, async (BlogService blogService) =>
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
}).DisableAntiforgery();

app.Run();
