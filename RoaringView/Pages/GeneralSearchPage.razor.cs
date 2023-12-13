using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RoaringView.Data;
using RoaringView.Model;
using Microsoft.Extensions.Logging; // Add this for logging
using Microsoft.AspNetCore.Components.Web;
using RoaringView.Service;

namespace RoaringView.Pages
{
    public partial class GeneralSearchPage
    {
        [Inject]
        public GeneralSearchService GeneralSearchService { get; set; }

        [Inject]
        public ILogger<GeneralSearchPage> Logger { get; set; }

        [Inject]
        private SortingService SortingService { get; set; }
        [Inject]
        NavigationManager NavigationManager { get; set; }

        
        private string currentSortColumn = null;
        private bool sortAscending = true;    
        [Parameter]
        public string SearchTerm { get; set; }
        private SearchResults searchResults;
        protected override async Task OnInitializedAsync()
        {
            try
            {
                Logger.LogInformation($"Initiating search for: {SearchTerm}");
                searchResults = await GeneralSearchService.SearchAsync(SearchTerm);
                Logger.LogInformation(searchResults != null
                    ? $"Search results retrieved for term '{SearchTerm}'"
                    : $"No search results found for term '{SearchTerm}'");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error occurred while searching for term '{SearchTerm}'");
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            await LoadSearchResults();
        }

        private async Task LoadSearchResults()
        {
            try
            {
                Logger.LogInformation($"Initiating search for: {SearchTerm}");
                searchResults = await GeneralSearchService.SearchAsync(SearchTerm);
                Logger.LogInformation(searchResults != null
                    ? $"Search results retrieved for term '{SearchTerm}'"
                    : $"No search results found for term '{SearchTerm}'");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error occurred while searching for term '{SearchTerm}'");
            }
        }


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


        // Display CompanyRatings Table
        public RenderFragment DisplayCompanyRatingsTable => BuildTableFragment(
            searchResults.CompanyRatings,
            new[] { "Rating", "Commentary" },
            rating => new object[] { rating.Rating, rating.Commentary }
        );
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
                    // Add cases for other lists as needed
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