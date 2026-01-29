using System.Diagnostics;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]/[action]")]
public class AccountController : Controller
{
    public IActionResult Login(string returnUrl = "/home")
    {
        var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
        .WithRedirectUri(returnUrl)
        .Build();

        return Challenge(authenticationProperties,Auth0Constants.AuthenticationScheme);
    }

    [Authorize]

    public IActionResult Logout()
    {
        var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
        .WithRedirectUri(Url.Content("~/"))
        .Build();

        return SignOut(authenticationProperties, Auth0Constants.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}