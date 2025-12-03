using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Portfolio.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class CommentTests : PageTest
{
    private string _baseUrl = "http://localhost:5050"; // dotnet run port

    [SetUp]
    public void Setup()
    {
        // App should be running on port 5050 via dotnet run
    }

    [Test]
    public async Task Comment_Without_Login_Should_Show_Auth_Required_Message()
    {
        // Navigate to a blog post
        await Page.GotoAsync($"{_baseUrl}/blog/building-sheetlink-excel-processor");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for page to load
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("Building SheetLink"));

        // Find the terminal input
        var terminalInput = Page.Locator("input.terminal-input").First;
        await Expect(terminalInput).ToBeVisibleAsync();

        // Try to post a comment without being logged in
        await terminalInput.FillAsync("comment \"This is a test comment\"");
        await terminalInput.PressAsync("Enter");

        // Wait for command output
        await Page.WaitForTimeoutAsync(1000);

        // Check for authentication required message in command output
        var output = Page.Locator(".command-output");
        if (await output.IsVisibleAsync())
        {
            var outputText = await output.TextContentAsync();
            Console.WriteLine($"Comment output: {outputText}");

            // Should show authentication required message
            Assert.That(
                outputText,
                Does.Contain("Authentication required").Or.Contain("login github"),
                "Should prompt user to login when trying to comment without authentication"
            );
        }
        else
        {
            Assert.Fail("No command output displayed after comment attempt");
        }
    }

    [Test]
    public async Task Comment_Command_Should_Show_In_Help()
    {
        // Navigate to a blog post
        await Page.GotoAsync($"{_baseUrl}/blog/building-sheetlink-excel-processor");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Find the terminal input
        var terminalInput = Page.Locator("input.terminal-input").First;

        // Type 'help' command
        await terminalInput.FillAsync("help");
        await terminalInput.PressAsync("Enter");

        // Wait for output
        await Page.WaitForTimeoutAsync(1000);

        // Check for comment command in help text
        var output = Page.Locator(".command-output");
        if (await output.IsVisibleAsync())
        {
            var helpText = await output.TextContentAsync();
            Console.WriteLine($"Help output length: {helpText?.Length}");

            // Verify comment command is documented
            Assert.That(helpText, Does.Contain("comment"));
            Assert.That(helpText, Does.Contain("Comments"));
        }
    }

    [Test]
    public async Task Comments_Section_Should_Be_Visible()
    {
        // Navigate to a blog post
        await Page.GotoAsync($"{_baseUrl}/blog/building-sheetlink-excel-processor");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check that comments section exists
        var commentsSection = Page.Locator(".comments-section");
        await Expect(commentsSection).ToBeVisibleAsync();

        // Check for comments header
        var commentsHeader = Page.Locator(".comments-header");
        await Expect(commentsHeader).ToBeVisibleAsync();

        var headerText = await commentsHeader.TextContentAsync();
        Console.WriteLine($"Comments header: {headerText}");

        // Should show comment count
        Assert.That(headerText, Does.Contain("Comments"));
    }

    [Test]
    public async Task Terminal_Input_Placeholder_Should_Show_Comment_Hint()
    {
        // Navigate to a blog post
        await Page.GotoAsync($"{_baseUrl}/blog/building-sheetlink-excel-processor");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Find the terminal input in the post page
        var terminalInput = Page.Locator("input.terminal-input").First;
        await Expect(terminalInput).ToBeVisibleAsync();

        // Check placeholder text mentions comment command
        var placeholder = await terminalInput.GetAttributeAsync("placeholder");
        Console.WriteLine($"Placeholder: {placeholder}");

        Assert.That(placeholder, Does.Contain("comment").IgnoreCase);
    }
}
