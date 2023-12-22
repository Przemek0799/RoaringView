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

        public DashboardService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = configuration["ApiBaseUrl"];
        }

        public async Task<SearchResults> GetCompanySpecificDataAsync(string roaringCompanyId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/CompanyData/{roaringCompanyId}");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SearchResults>(jsonString);
        }

        public async Task FetchAndSaveFinancialRecords(string roaringCompanyId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/RoaringFinancialRecord/fetchandsavefinancialrecords/{roaringCompanyId}", null);
            response.EnsureSuccessStatusCode();
            // You might want to handle the response here
        }

        public async Task FetchAndSaveCompanyInfo(string roaringCompanyId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/RoaringInfo/FetchAndSaveData/{roaringCompanyId}", null);
            response.EnsureSuccessStatusCode();
            // Handle response as needed
        }

        public async Task FetchAndSaveCompanyRating(string roaringCompanyId)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/FinancialRating/fetchandsavecompanyrating/{roaringCompanyId}", null);
            response.EnsureSuccessStatusCode();
            // Handle the response as needed
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
