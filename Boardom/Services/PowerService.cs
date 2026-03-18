using System.Reflection;
using Boardom.Components.Pages;
using Newtonsoft.Json;

namespace Boardom.Services;
public class Company
{
    [JsonProperty("name")]
    public string Name {get; set;}
}

public class PowerService
{
    private readonly ILogger<PowerService> _logger;
    private readonly HttpClient _httpClient;

    public string SelectedCompany = "No Selected";
    public double SelectedMaxPrice = 1.0;
    public List<string> AllCompanies = new List<string>();

    public PowerService(ILogger<PowerService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://stromligning.dk");
    }

    public async Task<bool> SaveSelectionAsync(string company, double maxPrice)
    {
        if (company == null || maxPrice == 0)
        {
            _logger.LogError("Invalid company or max price");
            return false;
        }   

        SelectedCompany = company;
        SelectedMaxPrice = maxPrice;
        return true;
    }

    public async Task<List<string>>GetCompaniesAsync()
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