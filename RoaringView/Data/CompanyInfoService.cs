using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace RoaringView.Data
{
    public class CompanyInfoService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CompanyInfoService> _logger;

        public CompanyInfoService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<CompanyInfoService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<CompanyInfo> GetCompanyInfoAsync()
        {
            var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            var response = await _httpClient.GetAsync("http://localhost:5091/api/RoaringSOInfo");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CompanyInfo>(jsonString);
        }
    }

    // Model to match the JSON response
    public class CompanyInfo
    {
        public List<Address> Addresses { get; set; }
        public List<Company> Companies { get; set; }
        public List<CompanyEmployee> CompanyEmployees { get; set; }
        // Other properties...
    }
}
