using Boardom.Components;
using Auth0.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddControllers();
builder.Services.AddHttpClient("DatabaseApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:DatabaseApiUrl"] ?? "http://localhost:8080/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddAuth0WebAppAuthentication(options => {
    options.Domain = builder.Configuration["Auth0:Domain"] ?? "";
    options.ClientId = builder.Configuration["Auth0:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"] ?? "";
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