using Microsoft.AspNetCore.Authentication;

namespace Boardom.Services;

public class ApiTokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiTokenService (IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task <string?> GetAccessTokenAsync()
    {
        return await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
    }

    public async Task AttachToken(HttpClient client)
    {
        string token = await GetAccessTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            return;
        }

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}