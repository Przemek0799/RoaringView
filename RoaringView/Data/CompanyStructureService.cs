﻿using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace RoaringView.Data
{
    public class CompanyStructureService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly string _apiBaseUrl;
        public CompanyStructureService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = configuration["ApiBaseUrl"];
        }
    
        public async Task<List<CompanyStructure>> GetCompanyStructuresAsync()
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/RoaringSOInfo");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CompanyStructure>>(jsonString);
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
