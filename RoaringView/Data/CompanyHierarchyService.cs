using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace RoaringView.Data
{
    public class CompanyHierarchyService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;
        private readonly ILogger<CompanyHierarchyService> _logger;

        public CompanyHierarchyService(HttpClient httpClient, IConfiguration configuration, ILogger<CompanyHierarchyService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = configuration["ApiBaseUrl"];
            _logger = logger;
        }

        public async Task<SearchResults> GetCompanySpecificDataAsync(string roaringCompanyId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/CompanyData/{roaringCompanyId}");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Received response: {jsonString}"); // Log the response

            return JsonConvert.DeserializeObject<SearchResults>(jsonString);
        }

        private void SetAuthorizationHeader()
        {
            var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }
    }
}
