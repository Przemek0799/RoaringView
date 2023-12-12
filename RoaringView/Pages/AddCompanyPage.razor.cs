using Microsoft.AspNetCore.Components;
using RoaringView.Data;
using RoaringView.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace RoaringView.Pages
{
    public partial class AddCompanyPage
    {
        [Inject]
        public CompanySearchService CompanySearchService { get; set; }

        [Inject]
        private ILogger<AddCompanyPage> Logger { get; set; }

        public string FreeText { get; set; }
        public RoaringSearchResult SearchResult { get; set; }

        private async Task SearchCompany()
        {
            SearchResult = await CompanySearchService.SearchByFreeTextAsync(FreeText);
            Logger.LogInformation($"Search results: {SearchResult}");
        }
        public RenderFragment BuildSearchResultsTable() => builder =>
        {
            if (SearchResult?.Hits != null && SearchResult.Hits.Count > 0)
            {
                int seq = 0;
                builder.OpenElement(seq++, "table");
                builder.AddAttribute(seq++, "class", "table");

                // Table Header
                builder.OpenElement(seq++, "thead");
                builder.OpenElement(seq++, "tr");
                var headers = new List<string> { "Company Name", "Legal Group Code", "Town", "Legal Group Text" };
                foreach (var header in headers)
                {
                    builder.OpenElement(seq++, "th");
                    builder.AddContent(seq++, header);
                    builder.CloseElement(); // th
                }
                builder.CloseElement(); // tr
                builder.CloseElement(); // thead

                // Table Body
                builder.OpenElement(seq++, "tbody");
                foreach (var hit in SearchResult.Hits)
                {
                    builder.OpenElement(seq++, "tr");
                    builder.AddContent(seq++, CreateCell(hit.CompanyName));
                    builder.AddContent(seq++, CreateCell(hit.LegalGroupCode));
                    builder.AddContent(seq++, CreateCell(hit.Town));
                    builder.AddContent(seq++, CreateCell(hit.LegalGroupText));
                    builder.CloseElement(); // tr
                }
                builder.CloseElement(); // tbody
                builder.CloseElement(); // table
            }
        };

        private RenderFragment CreateCell(string text) => (builder) =>
        {
            builder.OpenElement(0, "td");
            builder.AddContent(1, text);
            builder.CloseElement(); // td
        };
    }
}