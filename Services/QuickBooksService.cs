using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QuickBooksProxy.Models;

namespace QuickBooksProxy.Services;

public class QuickBooksService
{
    private readonly QuickBooksSettings _settings;
    private string? _accessToken;
    private string? _refreshToken;

    public QuickBooksService(IOptions<QuickBooksSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<TokenResponse> ExchangeCodeForTokenAsync(string code)
    {
        using var client = new HttpClient();

        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", _settings.RedirectUri }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer")
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);

        _accessToken = tokenResponse?.AccessToken;
        _refreshToken = tokenResponse?.RefreshToken;

        return tokenResponse!;
    }

    public string? GetAccessToken() => _accessToken;
}

