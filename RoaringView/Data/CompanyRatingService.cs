using Newtonsoft.Json;
using RoaringView.Model;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoaringView.Data
{
    public class CompanyRatingService
    {
        private readonly HttpClient _httpClient;

        public CompanyRatingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CompanyRating>> GetCompanyRatingsAsync()
        {
            var response = await _httpClient.GetAsync("http://localhost:5091/api/CompanyRatings");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CompanyRating>>(jsonString);
        }
    }
}
