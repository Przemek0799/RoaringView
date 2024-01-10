using Microsoft.AspNetCore.Components;
using RoaringView.Data;
using RoaringView.Model;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using RoaringView.Service;
using Microsoft.AspNetCore.Components.Rendering;

namespace RoaringView.Pages
{
    public partial class FilteredSearchPage
    {
        [Inject]
        public FilteredSearchService FilteredSearchService { get; set; }

        [Inject]
        public ILogger<FilteredSearchPage> _logger { get; set; }

        [Inject]
        private SortingService SortingService { get; set; }
        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        IJSRuntime JSRuntime { get; set; }
        private SearchResults searchResults;
        [Parameter]
        public string SearchTerm { get; set; }
        private string _lastUrl;
        private string currentSortColumn = null;
        private bool sortAscending = true;
        private List<Company> allCompanies = new List<Company>(); // Full list of companies
        private int currentPage = 1;
        private int itemsPerPage = 20;
        private int totalItems;
        private int totalPages;
        private bool CanNavigateForward => currentPage < totalPages;

        private bool CanNavigateBackward => currentPage > 1;

        protected override async Task OnInitializedAsync()
        {
            _logger.LogInformation("OnInitializedAsync called.");
            await FetchSearchResults();
        }

        protected override async Task OnParametersSetAsync()
        {
            var currentQueryString = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query;
            _logger.LogInformation($"OnParametersSetAsync called. Current Query String: {currentQueryString}");

            if (_lastUrl != currentQueryString)
            {
                _lastUrl = currentQueryString;
                _logger.LogInformation("Query string changed. Fetching new search results.");
                await FetchSearchResults();
            }
            else
            {
                _logger.LogInformation("Query string has not changed.");
            }
        }


        //code above to handle the changed url and to show a new filtered search, without it it wont refresh the page
        private async Task FetchSearchResults()
        {
            try
            {
                var uri = new Uri(NavigationManager.Uri);
                var queryParameters = QueryHelpers.ParseQuery(uri.Query);


                // If there is a query string, update SearchTerm, otherwise set it to an empty string
                SearchTerm = !string.IsNullOrEmpty(uri.Query) ? uri.Query.Substring(1).Replace("&", ", ") : "";


                // Extract query parameters
                var companyName = queryParameters.TryGetValue("companyName", out var companyNameValues) ? companyNameValues.FirstOrDefault() : null;
                var roaringCompanyId = queryParameters.TryGetValue("roaringCompanyId", out var roaringCompanyIdValues) ? roaringCompanyIdValues.FirstOrDefault() : null;
                DateTime? startDate = queryParameters.TryGetValue("startDate", out var startDateValues) && DateTime.TryParse(startDateValues.FirstOrDefault(), out var parsedStartDate) ? parsedStartDate : null;
                DateTime? endDate = queryParameters.TryGetValue("endDate", out var endDateValues) && DateTime.TryParse(endDateValues.FirstOrDefault(), out var parsedEndDate) ? parsedEndDate : null;
                int? minRating = queryParameters.TryGetValue("minRating", out var minRatingValues) && int.TryParse(minRatingValues.FirstOrDefault(), out var parsedMinRating) ? parsedMinRating : null;
                int? maxRating = queryParameters.TryGetValue("maxRating", out var maxRatingValues) && int.TryParse(maxRatingValues.FirstOrDefault(), out var parsedMaxRating) ? parsedMaxRating : null;

                _logger.LogInformation($"Performing filtered search with companyName: {companyName}, roaringCompanyId: {roaringCompanyId}, startDate: {startDate}, endDate: {endDate}, minRating: {minRating}, maxRating: {maxRating}");

                searchResults = await FilteredSearchService.SearchAsync(companyName, roaringCompanyId, startDate, endDate, minRating, maxRating);


                if (!string.IsNullOrWhiteSpace(companyName) || !string.IsNullOrWhiteSpace(roaringCompanyId) || startDate.HasValue || endDate.HasValue || minRating.HasValue || maxRating.HasValue)
                {
                    allCompanies = searchResults?.Companies?.ToList() ?? new List<Company>();
                    ApplyPagination();
                }
                else
                {
                    // Handle case when no search results or parameters are present
                    allCompanies.Clear();
                    ApplyPagination();
                }
                _logger.LogInformation($"Search Results Fetched: {JsonConvert.SerializeObject(searchResults)}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while performing filtered search.");
                searchResults = new SearchResults { Companies = new List<Company>() }; // Initialize on error
                ApplyPagination();
            }
            StateHasChanged();
        }


        private void GoToPage(int page)
        {
            currentPage = page;
            ApplyPagination();
        }

        private void NextPage() => GoToPage(Math.Min(currentPage + 1, totalPages));
        private void PreviousPage() => GoToPage(Math.Max(currentPage - 1, 1));
        private void FirstPage() => GoToPage(1);
        private void LastPage() => GoToPage(totalPages);

        private void SortData(string columnName, string listName)
        {
            if (!columnPropertyMappings.TryGetValue(columnName, out var propertyName))
            {
                _logger.LogWarning($"Invalid column name for sorting: {columnName}");
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
                    // Sort the full dataset
                    allCompanies = SortingService.SortData(allCompanies, propertyName, sortAscending);

                    // Reset pagination to the first page
                    currentPage = 1;

                    // Apply pagination to the sorted dataset
                    ApplyPagination();
                    break;
                    // Add cases for other lists as needed
            }

            StateHasChanged();
        }

        private void ApplyPagination()
        {
            totalItems = allCompanies.Count;
            totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            searchResults.Companies = allCompanies
                                      .Skip((currentPage - 1) * itemsPerPage)
                                      .Take(itemsPerPage).ToList();
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
                    _logger.LogInformation($"Building table for {typeof(T).Name} with {items.Count()} items.");
                    int seq = 0;
                    builder.OpenElement(seq++, "table");
                    builder.AddAttribute(seq++, "class", "table");

                    // Table Header with Sortable Columns
                    BuildTableHeaderOrFooter(builder, ref seq, headers, onHeaderClick, isHeader: true);

                    // Table Body with Clickable CompanyName
                    BuildTableBody(builder, ref seq, items, headers, valueSelector, navigateUrlSelector);

                    // Table Footer (mirroring the Header)
                    BuildTableHeaderOrFooter(builder, ref seq, headers, onHeaderClick, isHeader: false);

                    builder.CloseElement(); // Close table
                }
                else
                {
                    _logger.LogInformation($"No items to display for {typeof(T).Name}.");
                }
            };
        }

        private void BuildTableHeaderOrFooter(RenderTreeBuilder builder, ref int seq, string[] headers, Action<string> onHeaderClick, bool isHeader)
        {
            var tag = isHeader ? "thead" : "tfoot";

            builder.OpenElement(seq++, tag);
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
            builder.CloseElement(); // Close tag (thead or tfoot)
        }

        private void BuildTableBody<T>(
     RenderTreeBuilder builder,
     ref int seq,
     IEnumerable<T> items,
     string[] headers,
     Func<T, object[]> valueSelector,
     Func<T, string> navigateUrlSelector)
        {
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
        }
    }
}
