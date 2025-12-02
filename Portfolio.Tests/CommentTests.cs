using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Portfolio.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class CommentTests : PageTest
{
    private string _baseUrl = "http://localhost:5050"; 

    [SetUp]
    public void Setup()
    {
        // Assuming the app is running locally on port 5050 (or whatever port you configure)
        // In a real CI/CD pipeline, you'd start the app process here.
        // For this manual debugging session, ensure the app is running via `dotnet run` in another terminal.
    }

    [Test]
    public async Task Comment_Submission_Should_Work_Or_Log_Error()
    {
        // 1. Navigate to a blog post (e.g., the first one found or a specific test post)
        await Page.GotoAsync($"{_baseUrl}/blog/building-sheetlink-excel-processor");

        // 2. Wait for page to load
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("Building SheetLink"));

        // 3. Check if we need to login (Mocking or handling auth is tricky without UI interaction or tokens)
        // Since GitHub auth requires 3rd party interaction, for this specific "Circuit Terminated" debugging,
        // we assume we are testing the *unauthenticated* flow first OR the error happens *before* auth redirection if logic is broken.
        // However, the user said "When trying to comment". This implies they might be logged in or clicking the button triggers it.
        
        // Let's try to find the comment box.
        // If the user is NOT logged in, they should see a "Login with GitHub" button instead of a text area.
        
        var loginButton = Page.GetByRole(AriaRole.Link, new() { Name = "Login with GitHub to comment" });
        
        if (await loginButton.IsVisibleAsync())
        {
            Console.WriteLine("User is not logged in. Clicking login should redirect to GitHub.");
            // We can't easily automate GitHub login without credentials. 
            // But we can check if clicking it causes the crash.
            await loginButton.ClickAsync();
            
            // Wait a bit to see if crash happens
            await Page.WaitForTimeoutAsync(2000);
            
            // Check if we are redirected or still alive
            Console.WriteLine($"Current URL after login click: {Page.Url}");
        }
        else
        {
            // User is logged in (if we somehow shared state, which we didn't, so this block is unlikely reachable in fresh browser)
            // But let's assume we could script it.
            
            await Page.FillAsync("textarea.comment-input", "This is a test comment from Playwright.");
            await Page.ClickAsync("button.submit-comment");
            
            // Check for success message
            await Expect(Page.Locator(".comment-success")).ToBeVisibleAsync();
        }
    }
}
