using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using RoaringView.Model;
using Microsoft.Extensions.Logging;

namespace RoaringView.Data
{

    //fetches the search data for general search in navbar
    public class GeneralSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeneralSearchService> _logger;

        public GeneralSearchService(HttpClient httpClient, IConfiguration configuration, ILogger<GeneralSearchService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var apiBaseUrl = configuration["ApiBaseUrl"];
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                _httpClient.BaseAddress = new Uri(apiBaseUrl);
            }
            else
            {
                _logger.LogError("API base URL is not configured.");
                throw new InvalidOperationException("API base URL is not configured.");
            }
        }

        public async Task<SearchResults> SearchAsync(string searchTerm)
        {
            try
            {
                var relativeUri = $"/api/Search/{searchTerm}";
                _logger.LogInformation($"Sending search request to {relativeUri}");
                var response = await _httpClient.GetAsync(relativeUri);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SearchResults>(jsonString);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Error fetching search results");
                throw;
            }
        }
    }
}