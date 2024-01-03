using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace RoaringView.Data
{
    public class DashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<DashboardService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = configuration["ApiBaseUrl"];
            _logger = logger;
        }

        public async Task<SearchResults> GetCompanySpecificDataAsync(string roaringCompanyId)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/CompanyData/{roaringCompanyId}");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SearchResults>(jsonString);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching company specific data for {RoaringCompanyId}", roaringCompanyId);
                throw;
            }
        }

        public async Task FetchAndSaveFinancialRecords(string roaringCompanyId)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}/RoaringFinancialRecord/fetchandsavefinancialrecords/{roaringCompanyId}", null);
                response.EnsureSuccessStatusCode();
                // You might want to handle the response here
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching and saving financial records for {RoaringCompanyId}", roaringCompanyId);
                throw;
            }
        }

        public async Task FetchAndSaveCompanyInfo(string roaringCompanyId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/RoaringInfo/FetchAndSaveData/{roaringCompanyId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task FetchAndSaveCompanyRating(string roaringCompanyId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/FinancialRating/fetchandsavecompanyrating/{roaringCompanyId}", null);
            response.EnsureSuccessStatusCode();
        }

        private void SetAuthorizationHeader()
        {
            try
            {
                var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting authorization header");
            }
        }
    }
}
