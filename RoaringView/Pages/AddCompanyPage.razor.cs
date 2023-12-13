using Microsoft.AspNetCore.Components;
using RoaringView.Data;
using RoaringView.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;

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

        [Inject]
        NavigationManager NavigationManager { get; set; }
        private string currentSortColumn = null;
        private bool sortAscending = true;

        private async Task SearchCompany()
        {
            SearchResult = await CompanySearchService.SearchByFreeTextAsync(FreeText);
            Logger.LogInformation($"Search results: {SearchResult}");
        }

        // Add a method to handle the save action
        private async Task SaveCompany(string companyId)
        {
            try
            {
                await CompanySearchService.SaveCompanyDataAsync(companyId);
                Logger.LogInformation($"Company data saved for ID: {companyId}");
                // Optionally, refresh the page or display a success message
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error occurred while saving company data for ID: {companyId}");
            }
        }

        private void SortData(string columnName)
        {
            if (columnName == null) return;

            if (currentSortColumn == columnName)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = columnName;
                sortAscending = true;
            }

            // Implement the sorting logic based on columnName and sortAscending
            // This is a basic example, you may need to customize it based on your actual data structure
            if (SearchResult?.Hits != null)
            {
                SearchResult.Hits = sortAscending
                    ? SearchResult.Hits.OrderBy(c => GetPropertyValue(c, columnName)).ToList()
                    : SearchResult.Hits.OrderByDescending(c => GetPropertyValue(c, columnName)).ToList();
            }
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }


        // Display Companies Table with Sorting and Navigation
        public RenderFragment DisplayCompaniesTable => BuildTableFragment(
          SearchResult?.Hits,
            new[] { "CompanyName", "LegalGroupCode", "Town", "LegalGroupText" },
            company => new object[] { company.CompanyName, company.LegalGroupCode, company.Town, company.LegalGroupText },
            company => $"/Specific-company/{company.CompanyId}", // Assuming CompanyId is the identifier
            columnName => SortData(columnName)
        );

        private RenderFragment BuildTableFragment(IEnumerable<RoaringSearchResponse> items, string[] headers, Func<RoaringSearchResponse, object[]> valueSelector, Func<RoaringSearchResponse, string> navigateUrlSelector = null, Action<string> onHeaderClick = null)
        {
            return builder =>
            {
                if (items != null && items.Any())
                {
                    Logger.LogInformation($"Building table for RoaringSearchResponse with {items.Count()} items.");
                    int seq = 0;
                    builder.OpenElement(seq++, "table");
                    builder.AddAttribute(seq++, "class", "table");

                    // Table Header with Sortable Columns
                    builder.OpenElement(seq++, "thead");
                    builder.OpenElement(seq++, "tr");
                    foreach (var header in headers)
                    {
                        builder.OpenElement(seq++, "th");
                        if (onHeaderClick != null)
                        {
                            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => onHeaderClick(header)));
                        }
                        builder.AddContent(seq++, header);
                        builder.CloseElement(); // Close th
                    }
                    builder.CloseElement(); // Close tr
                    builder.CloseElement(); // Close thead

                    // Table Body
                    builder.OpenElement(seq++, "tbody");
                    foreach (var item in items)
                    {
                        builder.OpenElement(seq++, "tr");

                        var values = valueSelector(item);
                        foreach (var value in values)
                        {
                            builder.OpenElement(seq++, "td");
                            builder.AddContent(seq++, value?.ToString() ?? "N/A");
                            builder.CloseElement(); // Close td
                        }

                        // Add a save button to each row
                        builder.OpenElement(seq++, "td");
                        builder.OpenElement(seq++, "button");
                        builder.AddAttribute(seq++, "class", "btn btn-primary");
                        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => SaveCompany(item.CompanyId)));
                        builder.AddContent(seq++, "Save");
                        builder.CloseElement(); // Close button
                        builder.CloseElement(); // Close td

                        builder.CloseElement(); // Close tr
                    }
                    builder.CloseElement(); // Close tbody
                    builder.CloseElement(); // Close table
                }
                else
                {
                    Logger.LogInformation("No items to display for RoaringSearchResponse.");
                }
            };
        }
    }
}