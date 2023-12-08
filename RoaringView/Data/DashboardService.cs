using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Threading.Tasks;

// fetches most data from api for dashboard
namespace RoaringView.Data
{
    public class DashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public DashboardService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiBaseUrl"];
        }

        public async Task<SearchResults> GetCompanySpecificDataAsync(string roaringCompanyId)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/CompanyData/{roaringCompanyId}");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SearchResults>(jsonString);
        }
    }
}
