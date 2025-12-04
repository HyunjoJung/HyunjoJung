using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Portfolio.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class LoginTests : PageTest
{
    private string _baseUrl = "http://localhost:5050"; // dotnet run port

    [SetUp]
    public void Setup()
    {
        // App should be running in Docker on port 5051
    }

    [Test]
    public async Task Terminal_Login_Command_Should_Navigate()
    {
        // Navigate to homepage
        await Page.GotoAsync(_baseUrl);

        // Wait for page to load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Find the terminal input
        var terminalInput = Page.Locator("input.terminal-input").First;

        // Type 'login github' command
        await terminalInput.FillAsync("login github");
        await terminalInput.PressAsync("Enter");

        // Wait for navigation
        await Page.WaitForTimeoutAsync(2000);

        // Check if URL changed to /login or GitHub OAuth
        var currentUrl = Page.Url;
        Console.WriteLine($"Current URL after login command: {currentUrl}");

        // Should navigate to /login endpoint or GitHub OAuth page
        Assert.That(
            currentUrl.Contains("/login") || currentUrl.Contains("github.com"),
            $"Expected navigation to /login or GitHub, but got: {currentUrl}"
        );
    }

    [Test]
    public async Task Whoami_Command_Should_Show_Anonymous()
    {
        // Navigate to homepage
        await Page.GotoAsync(_baseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Find the terminal input
        var terminalInput = Page.Locator("input.terminal-input").First;

        // Type 'whoami' command
        await terminalInput.FillAsync("whoami");
        await terminalInput.PressAsync("Enter");

        // Wait for output
        await Page.WaitForTimeoutAsync(1000);

        // Check for command output
        var output = Page.Locator(".command-output");
        if (await output.IsVisibleAsync())
        {
            var outputText = await output.TextContentAsync();
            Console.WriteLine($"whoami output: {outputText}");
            Assert.That(outputText, Does.Contain("anonymous").Or.Contain("not authenticated"));
        }
    }

    [Test]
    public async Task Direct_Login_Endpoint_Should_Redirect_To_GitHub()
    {
        // Directly navigate to /login endpoint
        var response = await Page.GotoAsync($"{_baseUrl}/login");

        // Wait for redirect
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var currentUrl = Page.Url;
        Console.WriteLine($"URL after /login: {currentUrl}");

        // Should be redirected to GitHub (either /login or /login/oauth)
        Assert.That(
            currentUrl.Contains("github.com/login") && currentUrl.Contains("client_id"),
            $"Expected GitHub OAuth URL, but got: {currentUrl}"
        );
    }

    [Test]
    public async Task Terminal_Help_Command_Should_Show_Login_Info()
    {
        await Page.GotoAsync(_baseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var terminalInput = Page.Locator("input.terminal-input").First;

        await terminalInput.FillAsync("help");
        await terminalInput.PressAsync("Enter");

        await Page.WaitForTimeoutAsync(1000);

        var output = Page.Locator(".command-output");
        if (await output.IsVisibleAsync())
        {
            var helpText = await output.TextContentAsync();
            Console.WriteLine($"Help output: {helpText}");

            Assert.That(helpText, Does.Contain("login github"));
            Assert.That(helpText, Does.Contain("Authentication"));
        }
    }
}
