using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Linq;

namespace RoaringView.Data
{
    public class CompanySearchService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;
        private readonly ILogger<CompanySearchService> _logger;

        public CompanySearchService(HttpClient httpClient, IConfiguration configuration, ILogger<CompanySearchService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = configuration["ApiBaseUrl"];
            _logger = logger;
        }

        public async Task<RoaringSearchResult> SearchAsync(Dictionary<string, string> searchParams)
        {
            SetAuthorizationHeader();
            var queryParams = string.Join("&", searchParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/CompanySearch/search?{queryParams}");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Received response from API: {jsonString}");
            return JsonConvert.DeserializeObject<RoaringSearchResult>(jsonString);
        }

        public async Task<string> SaveCompanyDataAsync(string companyId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/CompanySearch/SaveCompany/{companyId}", null);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var savedCompanyResponse = JsonConvert.DeserializeObject<SavedCompanyResponse>(jsonString);
            return savedCompanyResponse.RoaringCompanyId;
        }

        private void SetAuthorizationHeader()
        {
            var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }

        public class SavedCompanyResponse
        {
            public string RoaringCompanyId { get; set; }
        }
    }
}
