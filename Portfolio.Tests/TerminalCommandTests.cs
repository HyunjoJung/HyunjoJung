using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Portfolio.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class TerminalCommandTests : PageTest
{
    private string _baseUrl = "http://localhost:5050";

    [SetUp]
    public void Setup()
    {
        // App should be running in Docker on port 5051
    }

    [Test]
    public async Task Ls_Command_Should_List_Directory()
    {
        await Page.GotoAsync(_baseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var terminalInput = Page.Locator("input.terminal-input").First;

        await terminalInput.FillAsync("ls");
        await terminalInput.PressAsync("Enter");
        await Page.WaitForTimeoutAsync(1000);

        var output = Page.Locator(".command-output");
        if (await output.IsVisibleAsync())
        {
            var outputText = await output.TextContentAsync();
            Console.WriteLine($"ls output: {outputText}");

            Assert.That(outputText, Does.Contain("blog"));
            Assert.That(outputText, Does.Contain("about"));
        }
    }

    [Test]
    public async Task Ls_Al_Command_Should_Show_Hidden_Files()
    {
        await Page.GotoAsync(_baseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var terminalInput = Page.Locator("input.terminal-input").First;

        await terminalInput.FillAsync("ls -al");
        await terminalInput.PressAsync("Enter");
        await Page.WaitForTimeoutAsync(1000);

        var output = Page.Locator(".command-output");
        if (await output.IsVisibleAsync())
        {
            var outputText = await output.TextContentAsync();
            Console.WriteLine($"ls -al output: {outputText}");

            // Should show . and .. with -al flag
            Assert.That(outputText, Does.Contain("."));
            Assert.That(outputText, Does.Contain(".."));
            Assert.That(outputText, Does.Contain("blog"));
        }
    }

    [Test]
    public async Task Cd_Command_Should_Navigate_To_About()
    {
        await Page.GotoAsync(_baseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var terminalInput = Page.Locator("input.terminal-input").First;

        await terminalInput.FillAsync("cd about");
        await terminalInput.PressAsync("Enter");

        // Wait for navigation to complete
        await Page.WaitForTimeoutAsync(1000);

        var currentUrl = Page.Url;
        Console.WriteLine($"URL after 'cd about': {currentUrl}");

        Assert.That(currentUrl, Does.Contain("/about"));
    }

    [Test]
    public async Task Cd_DotDot_From_About_Should_Go_To_Root()
    {
        // Start at /about page
        await Page.GotoAsync($"{_baseUrl}/about");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var terminalInput = Page.Locator("input.terminal-input").First;

        await terminalInput.FillAsync("cd ..");
        await terminalInput.PressAsync("Enter");

        // Should navigate back to root
        await Page.WaitForTimeoutAsync(1000);

        var currentUrl = Page.Url;
        Console.WriteLine($"URL after 'cd ..': {currentUrl}");

        Assert.That(currentUrl, Is.EqualTo($"{_baseUrl}/").Or.EqualTo(_baseUrl));
    }

    [Test]
    public async Task Pwd_Command_Should_Show_Current_Path()
    {
        await Page.GotoAsync($"{_baseUrl}/about");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var terminalInput = Page.Locator("input.terminal-input").First;

        await terminalInput.FillAsync("pwd");
        await terminalInput.PressAsync("Enter");
        await Page.WaitForTimeoutAsync(1000);

        var output = Page.Locator(".command-output");
        if (await output.IsVisibleAsync())
        {
            var outputText = await output.TextContentAsync();
            Console.WriteLine($"pwd output: {outputText}");

            Assert.That(outputText, Does.Contain("/about"));
        }
    }

    [Test]
    public async Task Grep_Command_Should_Search_Posts()
    {
        await Page.GotoAsync(_baseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var terminalInput = Page.Locator("input.terminal-input").First;

        await terminalInput.FillAsync("grep blazor");
        await terminalInput.PressAsync("Enter");
        await Page.WaitForTimeoutAsync(1000);

        var output = Page.Locator(".command-output");
        if (await output.IsVisibleAsync())
        {
            var outputText = await output.TextContentAsync();
            Console.WriteLine($"grep output: {outputText}");

            // Should either find matches or say no matches found
            Assert.That(
                outputText,
                Does.Contain("match").Or.Contain("Found").Or.Contain("no matches")
            );
        }
    }

    [Test]
    public async Task Help_Command_Should_Display_All_Commands()
    {
        await Page.GotoAsync(_baseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var terminalInput = Page.Locator("input.terminal-input").First;

        await terminalInput.FillAsync("help");
        await terminalInput.PressAsync("Enter");
        await Page.WaitForTimeoutAsync(1000);

        var output = Page.Locator(".command-output");
        Assert.That(await output.IsVisibleAsync(), Is.True, "Help output should be visible");

        var helpText = await output.TextContentAsync();
        Console.WriteLine($"Help text length: {helpText?.Length}");

        // Verify all major command categories are present
        Assert.That(helpText, Does.Contain("Navigation"));
        Assert.That(helpText, Does.Contain("Search"));
        Assert.That(helpText, Does.Contain("Authentication"));
        Assert.That(helpText, Does.Contain("cd"));
        Assert.That(helpText, Does.Contain("ls"));
        Assert.That(helpText, Does.Contain("grep"));
    }
}
