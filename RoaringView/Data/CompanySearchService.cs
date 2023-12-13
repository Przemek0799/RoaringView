using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RoaringView.Data
{
    public class CompanySearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly ILogger<CompanySearchService> _logger;

        public CompanySearchService(HttpClient httpClient, IConfiguration configuration, ILogger<CompanySearchService> logger)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiBaseUrl"];
            _logger = logger;
        }

        public async Task<RoaringSearchResult> SearchAsync(Dictionary<string, string> searchParams)
        {
            var queryParams = string.Join("&", searchParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/CompanySearch/search?{queryParams}");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Received response from API: {jsonString}");

            return JsonConvert.DeserializeObject<RoaringSearchResult>(jsonString);
        }


        public async Task SaveCompanyDataAsync(string companyId)
        {
            // Send a POST request to the API to save the company data
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/CompanySearch/SaveCompany/{companyId}", null);
            response.EnsureSuccessStatusCode();
            // Optionally handle the response
        }
    }
}
