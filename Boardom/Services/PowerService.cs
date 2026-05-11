using System.Reflection;
using System.Text;
using Boardom.Components.Pages;
using Newtonsoft.Json;
using Boardom.Models;

namespace Boardom.Services;


public class PowerService
{
  private readonly ILogger<PowerService> _logger;
  private readonly HttpClient _httpClient;
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ApiTokenService _tokenService;

  public string SelectionMode = "No Selected";
  public string SelectedCompany = "No Selected";
  public int Hours = 0;
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

  public async Task<bool> SaveSelectionAsync(PowerObject powObj)
  {
    if (string.IsNullOrWhiteSpace(powObj.Company) || powObj.MaxPrice == 0)
    {
      _logger.LogError("Invalid company or max price");
      return false;
    }

    try
    {
      HttpClient client = _httpClientFactory.CreateClient("PowerApi");

      powObj.UserId = await _tokenService.GetUserIdAsync();

      if (string.IsNullOrEmpty(powObj.UserId))
      {
        _logger.LogError("Could not retrieve user ID from access token");
        return false;
      }

      string json = JsonConvert.SerializeObject(powObj);
      StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

      HttpResponseMessage response = await client.PostAsync("/power-table", content);

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

    SelectionMode = powObj.Selection;
    SelectedCompany = powObj.Company;
    Hours = powObj.Hours;
    SelectedMaxPrice = powObj.MaxPrice;
    return true;
  }

  public async Task<PowerObject?> GetSavedSettingsAsync()
  {
    try
    {
      HttpClient client = _httpClientFactory.CreateClient("PowerApi");
      
      string? userId = await _tokenService.GetUserIdAsync();

      if (string.IsNullOrEmpty(userId))
      {
        _logger.LogError("Could not retrieve user ID from access token");
        return null;
      }

      _logger.LogInformation("Fetching power settings for userId: {UserId}", userId);

      HttpResponseMessage response = await client.GetAsync($"/power-table?userId={Uri.EscapeDataString(userId)}");
      string body = await response.Content.ReadAsStringAsync();

      PowerObject powObj = JsonConvert.DeserializeObject<List<PowerObject>>(body)[0];

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogWarning("Failed to get Power Settings. Code: {StatusCode} - Message: {Body}", response.StatusCode, body);
        return null;
      }

      _logger.LogInformation("Power settings response: {Raw}", body);

      return powObj;
      
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to fetch saved power settings: {Message}", ex.Message);
      return null;
    }
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

//======Charging Status =========//
  public async Task <bool> GetChargingStatusAsync()
  {
    try
    {
      string? userId = await _tokenService.GetUserIdAsync();
      if (string.IsNullOrEmpty(userId))
      {
        _logger.LogError("Could not retrieve user ID from access token");
        return false;
      }

      HttpClient client = _httpClientFactory.CreateClient("PowerApi");

      HttpResponseMessage response = await client.GetAsync($"/charging?userId={Uri.EscapeDataString(userId)}");

      string body = await response.Content.ReadAsStringAsync();
      _logger.LogInformation("Charging status response: {StatusCode} - {Body}", response.StatusCode, body); 

      if (!response.IsSuccessStatusCode)
      {
        _logger.LogError("Failed to get charging status. Code: {StatusCode}", response.StatusCode);
        return false;
      }  

      // string body = await response.Content.ReadAsStringAsync();
      var jsonObject = JsonConvert.DeserializeObject<dynamic>(body);
      bool isCharging = jsonObject.isCharging ?? jsonObject?.charging ?? false;
      return isCharging;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to fetch charging status: {Message}", ex.Message);
      return false;
    }
  }
}

