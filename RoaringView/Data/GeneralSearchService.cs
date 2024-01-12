using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using RoaringView.Model;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace RoaringView.Data
{
    public class GeneralSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeneralSearchService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GeneralSearchService(HttpClient httpClient, IConfiguration configuration, ILogger<GeneralSearchService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

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
                var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                }

                var relativeUri = $"/api/GeneralSearch/{searchTerm}";
                _logger.LogInformation("Sending search request to {relativeUri}",relativeUri);
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
