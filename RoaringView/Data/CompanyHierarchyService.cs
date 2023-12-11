using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Threading.Tasks;


//  hämtar company structure från api , förutom top company då den aldrig skrivs in i company structure, dock finns det referenser till den med companyid    public class CompanyHierarchyService

namespace RoaringView.Data
{
    public class CompanyHierarchyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly ILogger<CompanyHierarchyService> _logger; 

        public CompanyHierarchyService(HttpClient httpClient, IConfiguration configuration, ILogger<CompanyHierarchyService> logger)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiBaseUrl"];
            _logger = logger; 
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
