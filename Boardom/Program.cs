using Boardom.Components;
using Auth0.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddzRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddControllers();
builder.Services.AddHttpClient("DatabaseApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:DatabaseApiUrl"] ?? "http://localhost:8080/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var auth0Domain = builder.Configuration["Auth0:Domain"];
if (string.IsNullOrWhiteSpace(auth0Domain))
{
    throw new InvalidOperationException("Auth0 configuration error: 'Auth0:Domain' is missing or empty.");
}
var auth0ClientId = builder.Configuration["Auth0:ClientId"];
if (string.IsNullOrWhiteSpace(auth0ClientId))
{
    throw new InvalidOperationException("Auth0 configuration error: 'Auth0:ClientId' is missing or empty.");
}
var auth0ClientSecret = builder.Configuration["Auth0:ClientSecret"];
if (string.IsNullOrWhiteSpace(auth0ClientSecret))
{
    throw new InvalidOperationException("Auth0 configuration error: 'Auth0:ClientSecret' is missing or empty.");
}
builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = auth0Domain;
    options.ClientId = auth0ClientId;
    options.ClientSecret = auth0ClientSecret;

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

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();