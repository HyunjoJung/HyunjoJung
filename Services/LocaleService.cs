using Microsoft.JSInterop;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace Portfolio.Services;

public class LocaleService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocaleService(IJSRuntime jsRuntime, IHttpContextAccessor httpContextAccessor)
    {
        _jsRuntime = jsRuntime;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SetCultureAsync(string culture)
    {
        // Set cookie for persistent culture
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            ".AspNetCore.Culture",
            $"c={culture}|uic={culture}",
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.Lax
            }
        );

        // Refresh the page with the new culture
        var currentUrl = await _jsRuntime.InvokeAsync<string>("eval", "window.location.href");
        var uri = new Uri(currentUrl);

        // Remove existing culture query parameter if present
        var path = uri.GetLeftPart(UriPartial.Path);

        await _jsRuntime.InvokeVoidAsync("eval", $"window.location.href = '{path}?culture={culture}'");
    }

    public string GetCurrentCultureCode()
    {
        return CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    }

    public string GetOtherCultureCode()
    {
        var current = GetCurrentCultureCode();
        return current == "ko" ? "en" : "ko";
    }

    public string GetOtherCultureName()
    {
        var current = GetCurrentCultureCode();
        return current == "ko" ? "English" : "한국어";
    }
}
