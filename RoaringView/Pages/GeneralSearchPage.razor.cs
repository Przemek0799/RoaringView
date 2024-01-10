using Microsoft.AspNetCore.Components;
using RoaringView.Data;
using RoaringView.Model;
using Microsoft.AspNetCore.Components.Web;
using RoaringView.Service;

namespace RoaringView.Pages
{
    public partial class GeneralSearchPage
    {
        [Inject]
        public GeneralSearchService GeneralSearchService { get; set; }

        [Inject]
        public ILogger<GeneralSearchPage> _logger { get; set; }

        [Inject]
        private SortingService SortingService { get; set; }
        [Inject]
        NavigationManager NavigationManager { get; set; }

        
        private string currentSortColumn = null;
        private bool sortAscending = true;    
        [Parameter]
        public string SearchTerm { get; set; }
        private SearchResults searchResults;
        private List<Company> allCompanies = new List<Company>(); // Full list of companies
        private int currentPage = 1;
        private int itemsPerPage = 10; 
        private int totalItems;
        private int totalPages;
        private bool CanNavigateForward => currentPage < totalPages;

        private bool CanNavigateBackward => currentPage > 1;

        protected override async Task OnInitializedAsync()
        {
            await FetchSearchResults();
        }

        protected override async Task OnParametersSetAsync()
        {
            // Only fetch new results if the search term changes
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                await FetchSearchResults();
            }
        }

        private async Task FetchSearchResults()
        {
            try
            {
                _logger.LogInformation($"Initiating search for: {SearchTerm}");
                searchResults = await GeneralSearchService.SearchAsync(SearchTerm);
                if (searchResults != null)
                {
                    allCompanies = searchResults.Companies.ToList(); // Store all companies
                    ApplyPagination(); // Apply initial pagination
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching search results");
            }
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
            searchResults.Companies = allCompanies.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
            StateHasChanged();
        }

        // Display CompanyRatings Table
        public RenderFragment DisplayCompanyRatingsTable => BuildTableFragment(
            searchResults.CompanyRatings,
            new[] { "Rating", "Commentary" },
            rating => new object[] { rating.Rating, rating.Commentary }
        );
        // Display CompanyEmployees Table
        public RenderFragment DisplayCompanyEmployeesTable => BuildTableFragment(
            searchResults.CompanyEmployees,
            new[] { "ID", "Name", "Role" },
            employee => new object[] { employee.EmployeeInCompanyId, employee.TopDirectorName, employee.TopDirectorFunction }
        );

        // Display Companies Table with Sorting and Navigation
        public RenderFragment DisplayCompaniesTable => BuildTableFragment(
            searchResults.Companies,
            new[] { "CompanyId", "CompanyName", "Organization Number" },
            company => new object[] { company.CompanyId, company.CompanyName, company.RoaringCompanyId },
            company => $"/Specific-company/{company.RoaringCompanyId}",
            columnName => SortData(columnName, "Companies")
        );

        private readonly Dictionary<string, string> columnPropertyMappings = new Dictionary<string, string>
{
    { "CompanyId", nameof(Company.CompanyId) },
    { "CompanyName", nameof(Company.CompanyName) },
    { "Organization Number", nameof(Company.RoaringCompanyId) }
    // Add other mappings as necessary
};

        // Generic method to build table for any entity
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
                    _logger.LogInformation($"No items to display for {typeof(T).Name}.");
                }
            };
        }


      

    }
}