using Boardom.Components;
using Auth0.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
});

builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme,
    options =>
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.HttpOnly = true;
    });

builder.Services.AddHttpClient("DatabaseApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:DatabaseApiUrl"]);
    client.Timeout = TimeSpan.FromSeconds(10);
});
string auth0Domain = builder.Configuration["Auth0:Domain"];
if (string.IsNullOrWhiteSpace(auth0Domain))
{
    throw new InvalidOperationException("Auth0 configuration error: 'Auth0:Domain' is missing or empty.");
}
string auth0ClientId = builder.Configuration["Auth0:ClientId"];
if (string.IsNullOrWhiteSpace(auth0ClientId))
{
    throw new InvalidOperationException("Auth0 configuration error: 'Auth0:ClientId' is missing or empty.");
}
string auth0ClientSecret = builder.Configuration["Auth0:ClientSecret"];
if (string.IsNullOrWhiteSpace(auth0ClientSecret))
{
    throw new InvalidOperationException("Auth0 configuration error: 'Auth0:ClientSecret' is missing or empty.");
}
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = auth0Domain;
    options.ClientId = auth0ClientId;
    options.ClientSecret = auth0ClientSecret;

     options.OpenIdConnectEvents = new OpenIdConnectEvents
    {
        OnRemoteFailure = context =>
        {
            if (context.Failure.Message != null && 
            context.Failure.Message.Contains ("access_denied", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/");
                context.HandleResponse();
            }
            return Task.CompletedTask;
        }
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
}
);

app.UseStaticFiles();


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();