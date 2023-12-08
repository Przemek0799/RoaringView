using Newtonsoft.Json;
using RoaringView.Model;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoaringView.Data
{
    public class FinancialRecordService
    {
        private readonly HttpClient _httpClient;

        public FinancialRecordService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FinancialRecord>> GetFinancialRecordsAsync()
        {
            var response = await _httpClient.GetAsync("http://localhost:5091/api/FinancialRecords");
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<FinancialRecord>>(jsonString);
        }
    }
}
