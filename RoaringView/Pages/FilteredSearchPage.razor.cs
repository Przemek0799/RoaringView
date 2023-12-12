using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RoaringView.Data;
using RoaringView.Model;
using Microsoft.Extensions.Logging; // Add this for logging
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using RoaringView.Service;

namespace RoaringView.Pages
{
    public partial class FilteredSearchPage
    {
        [Inject]
        public FilteredSearchService FilteredSearchService { get; set; }

        [Inject]
        public ILogger<FilteredSearchPage> Logger { get; set; } // Inject Logger

        [Parameter]
        public string SearchTerm { get; set; }
        private string _lastUrl;
        [Inject]
        private SortingService SortingService { get; set; }
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        IJSRuntime JSRuntime { get; set; }
        private SearchResults searchResults;

        private string currentSortColumn = null;
        private bool sortAscending = true;

        protected override void OnInitialized()
        {
            NavigationManager.LocationChanged += HandleLocationChanged;
        }
        private async void HandleLocationChanged(object sender, LocationChangedEventArgs e)
        {
            await JSRuntime.InvokeVoidAsync("location.reload");
        }
        public void Dispose()
        {
            NavigationManager.LocationChanged -= HandleLocationChanged;
        }
        protected override async Task OnParametersSetAsync()
        {
            var currentUrl = NavigationManager.Uri;
            if (_lastUrl != currentUrl)
            {
                _lastUrl = currentUrl;
                await LoadSearchResults();
            }
        }

        //code above to handle the changed url and to show a new filtered search, without it it wont refresh the page
        private async Task LoadSearchResults()
        {
            try
            {
                var uri = new Uri(NavigationManager.Uri);
                var queryParameters = QueryHelpers.ParseQuery(uri.Query);

                var companyName = queryParameters.TryGetValue("companyName", out var companyNameValues) ? companyNameValues.FirstOrDefault() : null;
                var roaringCompanyId = queryParameters.TryGetValue("roaringCompanyId", out var roaringCompanyIdValues) ? roaringCompanyIdValues.FirstOrDefault() : null;
                DateTime? startDate = queryParameters.TryGetValue("startDate", out var startDateValues) && DateTime.TryParse(startDateValues.FirstOrDefault(), out var parsedStartDate) ? parsedStartDate : null;
                DateTime? endDate = queryParameters.TryGetValue("endDate", out var endDateValues) && DateTime.TryParse(endDateValues.FirstOrDefault(), out var parsedEndDate) ? parsedEndDate : null;
                int? minRating = queryParameters.TryGetValue("minRating", out var minRatingValues) && int.TryParse(minRatingValues.FirstOrDefault(), out var parsedMinRating) ? parsedMinRating : null;
                int? maxRating = queryParameters.TryGetValue("maxRating", out var maxRatingValues) && int.TryParse(maxRatingValues.FirstOrDefault(), out var parsedMaxRating) ? parsedMaxRating : null;

                Logger.LogInformation($"Performing filtered search with companyName: {companyName}, roaringCompanyId: {roaringCompanyId}, startDate: {startDate}, endDate: {endDate}, minRating: {minRating}, maxRating: {maxRating}");

                if (!string.IsNullOrWhiteSpace(companyName) || !string.IsNullOrWhiteSpace(roaringCompanyId) || startDate.HasValue || endDate.HasValue || minRating.HasValue || maxRating.HasValue)
                {
                    searchResults = await FilteredSearchService.SearchAsync(companyName, roaringCompanyId, startDate, endDate, minRating, maxRating);
                    Logger.LogInformation($"Search results retrieved: {JsonConvert.SerializeObject(searchResults)}");
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while performing filtered search.");
            }
        }



        //SortData method to use the SortingService and update the UI:
        private void SortData(string columnName, string listName)
        {
            if (!columnPropertyMappings.TryGetValue(columnName, out var propertyName))
            {
                Logger.LogWarning($"Invalid column name for sorting: {columnName}");
                return;
            }
            if (currentSortColumn == columnName)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = columnName;
                sortAscending = true;
            }

            switch (listName)
            {
                case "Companies":
                    searchResults.Companies = SortingService.SortData(searchResults.Companies, propertyName, sortAscending);
                    break;
                    // ...
            }

            StateHasChanged();
        }

        private readonly Dictionary<string, string> columnPropertyMappings = new Dictionary<string, string>
{
    { "CompanyId", nameof(Company.CompanyId) },
    { "CompanyName", nameof(Company.CompanyName) },
    { "Organization Number", nameof(Company.RoaringCompanyId) }
    // Add other mappings as necessary
};


        // Display Companies Table with Sorting and Navigation
        public RenderFragment DisplayCompaniesTable => BuildTableFragment(
            searchResults.Companies,
            new[] { "CompanyId", "CompanyName", "Organization Number" },
            company => new object[] { company.CompanyId, company.CompanyName, company.RoaringCompanyId },
            company => $"/Specific-company/{company.RoaringCompanyId}", 
            columnName => SortData(columnName, "Companies")
        );



        private RenderFragment BuildTableFragment<T>(
    IEnumerable<T> items,
    string[] headers,
    Func<T, object[]> valueSelector,
    Func<T, string> navigateUrlSelector = null,
    Action<string> onHeaderClick = null)
        {
            return builder =>
            {
                if (items != null && items.Any())
                {
                    Logger.LogInformation($"Building table for {typeof(T).Name} with {items.Count()} items.");
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

                    // Table Body with Clickable CompanyName
                    builder.OpenElement(seq++, "tbody");
                    foreach (var item in items)
                    {
                        builder.OpenElement(seq++, "tr");

                        var values = valueSelector(item);
                        for (int i = 0; i < values.Length; i++)
                        {
                            var value = values[i];
                            bool isCompanyName = headers[i] == "CompanyName";

                            if (isCompanyName && navigateUrlSelector != null)
                            {
                                // Apply clickable behavior only to CompanyName
                                string navigateUrl = navigateUrlSelector(item);
                                builder.OpenElement(seq++, "td");
                                builder.AddAttribute(seq++, "class", "clickable-cell");
                                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigationManager.NavigateTo(navigateUrl)));
                            }
                            else
                            {
                                builder.OpenElement(seq++, "td");
                            }

                            builder.AddContent(seq++, value?.ToString() ?? "N/A");
                            builder.CloseElement(); // Close td
                        }

                        builder.CloseElement(); // Close tr
                    }
                    builder.CloseElement(); // Close tbody
                    builder.CloseElement(); // Close table

                }
                else
                {
                    Logger.LogInformation($"No items to display for {typeof(T).Name}.");
                }
            };
        }
    }
}