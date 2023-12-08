using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoaringView.Data
{
    public class CompanyInfoService
    {
        private readonly HttpClient _httpClient;

        public CompanyInfoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CompanyInfo> GetCompanyInfoAsync()
        {
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
