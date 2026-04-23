using System.Reflection;
using System.Text;
using Boardom.Components.Pages;
using Newtonsoft.Json;

namespace Boardom.Services;

public class Company
{
  [JsonProperty("name")]
  public string Name { get; set; }
}

public class PowerService
{
  private readonly ILogger<PowerService> _logger;
  private readonly HttpClient _httpClient;
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ApiTokenService _tokenService;

  public string SelectedCompany = "No Selected";
  public double SelectedMaxPrice = 1.0;
  public List<string> AllCompanies = new List<string>();

  public PowerService(ILogger<PowerService> logger, HttpClient httpClient, IHttpClientFactory httpClientFactory, ApiTokenService tokenService)
  {
    _logger = logger;
    _httpClient = httpClient;
    _httpClient.BaseAddress = new Uri("https://stromligning.dk");
    _httpClientFactory = httpClientFactory;
    _tokenService = tokenService;
  }

  public async Task<bool> SaveSelectionAsync(string company, double maxPrice)
  {
    if (string.IsNullOrWhiteSpace(company) || maxPrice == 0)
    {
      _logger.LogError("Invalid company or max price");
      return false;
    }

    try
    {
      var client = _httpClientFactory.CreateClient("PowerApi");
      string? userId = await _tokenService.GetUserIdAsync();
      if (string.IsNullOrEmpty(userId))
      {
        _logger.LogError("Could not retrieve user ID from access token");
        return false;
      }
      var payload = new { company, price = maxPrice, userId };
      var json = JsonConvert.SerializeObject(payload);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      var response = await client.PostAsync("/power-table", content);

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to save power settings. Status: {StatusCode}", response.StatusCode);
        return false;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to post power settings to Go API");
      return false;
    }

    SelectedCompany = company;
    SelectedMaxPrice = maxPrice;
    return true;
  }

  public async Task<List<string>> GetCompaniesAsync()
  {
    List<string> companies = new List<string>();

    try
    {
      string raw = await _httpClient.GetStringAsync("api/companies?yearlyConsumption=5000&region=DK1&periodMonths=12");

      List<Company> response = JsonConvert.DeserializeObject<List<Company>>(raw);

      foreach (Company comp in response)
      {
        companies.Add(comp.Name);
      }

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to fetch companies");
    }

    AllCompanies = companies;

    return companies;
  }
}
