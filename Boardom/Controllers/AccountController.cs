using System.Diagnostics;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]/[action]")]
public class AccountController : Controller
{
    public async Task Login(string returnUrl = "/home")
    {
        var AuthenticationProperties = new LoginAuthenticationPropertiesBuilder()
        .WithRedirectUri(returnUrl)
        .Build();

        await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, AuthenticationProperties);
    }

    [Authorize]
    public async Task Logout()
    {
        var AuthenticationProperties = new LogoutAuthenticationPropertiesBuilder()
        .WithRedirectUri(Url.Action("Index", "Home"))
        .Build();

        await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, AuthenticationProperties);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}