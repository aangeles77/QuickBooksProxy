using Microsoft.AspNetCore.Mvc;
using QuickBooksProxy.Services;
using System.Net.Http.Headers;
using System.Text;

namespace QuickBooksProxy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuickBooksAuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly QuickBooksService? _qbService;

    public QuickBooksAuthController(IHttpClientFactory httpClientFactory, QuickBooksService qbService)
    {
        _httpClientFactory = httpClientFactory;
        _qbService = qbService;
    }

    [HttpPost("exchange")]
    public async Task<IActionResult> ExchangeToken([FromBody] TokenRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = request.Code,
            ["redirect_uri"] = request.RedirectUri
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer")
        {
            Content = new FormUrlEncodedContent(form)
        };

        string authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{request.ClientId}:{request.ClientSecret}"));
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var response = await client.SendAsync(httpRequest);
        var json = await response.Content.ReadAsStringAsync();
        return Content(json, "application/json");
    }
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("pong");
    }


    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var token = await _qbService!.ExchangeCodeForTokenAsync(code);
        return Ok("Authentication complete. You may close this window.");
    }

    [HttpGet("token")]
    public IActionResult GetToken()
    {
        var token = _qbService!.GetAccessToken();
        if (token == null) return Unauthorized();
        return Ok(new { accessToken = token });
    }
}

public class TokenRequest
{
    public string Code { get; set; } = "";
    public string RedirectUri { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
}
