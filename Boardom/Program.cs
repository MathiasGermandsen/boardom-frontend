using Boardom.Components;
using Auth0.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using System.Net;
using Boardom.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// application services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<PendingDeviceStore>();
builder.Services.AddScoped<PowerService>();
builder.Services.AddScoped<DeviceFunctions>();
builder.Services.AddScoped<DataFunctions>();
builder.Services.AddScoped<GroupFunctions>();
builder.Services.AddScoped<ApiTokenService>();
builder.Services.AddScoped<DeviceStatus>();

builder.Services.AddResponseCompression(options =>
{
  options.EnableForHttps = true;
  options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
      new[] { "text/css", "application/javascript", "text/javascript" });
});


builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

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
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
      options.Cookie.HttpOnly = true;
    });

builder.Services.AddHttpClient("DatabaseApi", client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiSettings:DatabaseApiUrl"]!);
  client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("PowerApi", client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ApiSettings:PowerApiUrl"]!);
  client.Timeout = TimeSpan.FromSeconds(10);
});

string? auth0Domain = builder.Configuration["Auth0:Domain"];

if (string.IsNullOrWhiteSpace(auth0Domain))
{
  throw new InvalidOperationException("Auth0 configuration error: 'Auth0:Domain' is missing or empty.");
}

string? auth0ClientId = builder.Configuration["Auth0:ClientId"];

if (string.IsNullOrWhiteSpace(auth0ClientId))
{
  throw new InvalidOperationException("Auth0 configuration error: 'Auth0:ClientId' is missing or empty.");
}
string? auth0ClientSecret = builder.Configuration["Auth0:ClientSecret"];

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
      if (context.Failure!.Message != null &&
          context.Failure.Message.Contains("access_denied", StringComparison.OrdinalIgnoreCase))
      {
        context.Response.Redirect("/");
        context.HandleResponse();
      }
      return Task.CompletedTask;
    }
  };
})
.WithAccessToken(options =>
{
  options.Audience = builder.Configuration["Auth0:Audience"];
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Response compression must be first
app.UseResponseCompression();

// Support reverse proxy (Cloudflare Tunnel)
// Clear default limits so headers from any proxy are accepted
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
  app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
  context.Response.Headers.Append("X-Frame-Options", "DENY");
  context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
  context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
  await next();
}
);

app.UseStaticFiles(new StaticFileOptions
{
  OnPrepareResponse = ctx =>
  {
    // Cache static files for 1 hour, but allow revalidation
    ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600, must-revalidate");
  }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
