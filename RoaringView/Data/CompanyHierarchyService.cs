using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Threading.Tasks;

// fetches most data from api for dashboard
namespace RoaringView.Data
{
    public class CompanyHierarchyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly ILogger<CompanyHierarchyService> _logger; // Inject Logger

        public CompanyHierarchyService(HttpClient httpClient, IConfiguration configuration, ILogger<CompanyHierarchyService> logger)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiBaseUrl"];
            _logger = logger; // Initialize logger
        }

        public async Task<SearchResults> GetCompanySpecificDataAsync(string roaringCompanyId)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/CompanyData/{roaringCompanyId}");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Received response: {jsonString}"); // Log the response

            return JsonConvert.DeserializeObject<SearchResults>(jsonString);
        }
    }
}
