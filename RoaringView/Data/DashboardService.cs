using Microsoft.AspNetCore.Mvc;
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
        public async Task FetchAndSaveFinancialRecords(string roaringCompanyId)
        {
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/RoaringFinancialRecord/fetchandsavefinancialrecords/{roaringCompanyId}", null);
            response.EnsureSuccessStatusCode();
            // You might want to handle the response here
        }

        public async Task FetchAndSaveCompanyInfo(string roaringCompanyId)
        {
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/RoaringInfo/FetchAndSaveData/{roaringCompanyId}", null);
            response.EnsureSuccessStatusCode();
            
           
        }

        public async Task FetchAndSaveCompanyRating(string roaringCompanyId)
        {
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/FinancialRating/fetchandsavecompanyrating/{roaringCompanyId}", null);
            response.EnsureSuccessStatusCode();
            // Handle the response as needed
        }


    }
}
