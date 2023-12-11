﻿using Newtonsoft.Json;
using RoaringView.Model;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RoaringView.Data
{

    //  hämtar company structure från api , förutom top company då den aldrig skrivs in i company structure, dock finns det referenser till den med companyid
    public class CompanyStructureService
    {
        private readonly HttpClient _httpClient;

        public CompanyStructureService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CompanyStructure>> GetCompanyStructuresAsync()
        {
            var response = await _httpClient.GetAsync("http://localhost:5091/api/CompanyStructures");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CompanyStructure>>(jsonString);
        }
    }
}
