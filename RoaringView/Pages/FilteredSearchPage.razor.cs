using Microsoft.AspNetCore.Components;
using RoaringView.Data;
using RoaringView.Model;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Components.Routing;
using RoaringView.Service;

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
        public BuildTableService TableBuilder { get; set; }
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
            _logger.LogInformation("Initial search results fetched.");
        }

        // OnInitialized och OnLocationChanged kollar om url har ändrats och gör ett nyt search vid behov, Dispose "method to prevent memory leaks."
        protected override void OnInitialized()
        {
            NavigationManager.LocationChanged += OnLocationChanged;
            base.OnInitialized();
        }
        private async void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            _logger.LogInformation("URL changed. New URL: " + e.Location);
            await FetchSearchResults();
        }
        public void Dispose()
{
    NavigationManager.LocationChanged -= OnLocationChanged;
}
        protected override async Task OnParametersSetAsync()
        {
            var currentQueryString = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query;
            _logger.LogInformation($"OnParametersSetAsync called. Current Query String: {currentQueryString}");

            if (_lastUrl != currentQueryString)
            {
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
            _logger.LogInformation("Starting FetchSearchResults");

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

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while performing filtered search.");
                searchResults = new SearchResults { Companies = new List<Company>() }; // Initialize on error
                ApplyPagination();
            }
            _logger.LogInformation($"Search Results Fetched: {JsonConvert.SerializeObject(searchResults)}");

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
        private RenderFragment DisplayCompaniesTable => TableBuilder.BuildTableFragment(
         searchResults.Companies,
         new[] { "CompanyId", "CompanyName", "Organization Number" },
         company => new object[] { company.CompanyId, company.CompanyName, company.RoaringCompanyId },
         NavigationManager,
         company => $"/Specific-company/{company.RoaringCompanyId}",
         columnName => SortData(columnName, "Companies")
     );

    }
}
