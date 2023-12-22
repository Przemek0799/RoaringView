using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using RoaringView.Model;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoaringView.Data
{
    public class FilteredSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FilteredSearchService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FilteredSearchService(HttpClient httpClient, IConfiguration configuration, ILogger<FilteredSearchService> logger, IHttpContextAccessor httpContextAccessor)
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

        public async Task<SearchResults> SearchAsync(string? companyName = null, string? roaringCompanyId = null, DateTime? startDate = null, DateTime? endDate = null, int? minRating = null, int? maxRating = null)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(companyName))
                queryParams.Add($"companyName={Uri.EscapeDataString(companyName)}");
            if (!string.IsNullOrWhiteSpace(roaringCompanyId))
                queryParams.Add($"roaringCompanyId={Uri.EscapeDataString(roaringCompanyId)}");
            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value.ToString("yyyy-MM-dd")}");
            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value.ToString("yyyy-MM-dd")}");
            if (minRating.HasValue)
                queryParams.Add($"minRating={minRating.Value}");
            if (maxRating.HasValue)
                queryParams.Add($"maxRating={maxRating.Value}");
            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;
            var relativeUri = $"/api/FilteredSearch{queryString}";

            try
            {
                var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["jwtToken"];
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                }

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
