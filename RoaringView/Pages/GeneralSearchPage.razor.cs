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

        [Parameter]
        public string SearchTerm { get; set; }

        private SearchResults searchResults;
        private string currentSortColumn = null;
        private bool sortAscending = true;
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
                    searchResults.Companies = SortingService.SortData(searchResults.Companies, columnName, sortAscending);
                    break;
                    // Add cases for other lists if needed
            }

            StateHasChanged();
        }

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

                    // Table Body with Clickable Rows
                    builder.OpenElement(seq++, "tbody");
                    foreach (var item in items)
                    {
                        builder.OpenElement(seq++, "tr");
                        if (navigateUrlSelector != null)
                        {
                            string navigateUrl = navigateUrlSelector(item);
                            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigationManager.NavigateTo(navigateUrl)));
                        }
                        var values = valueSelector(item);
                        foreach (var value in values)
                        {
                            builder.AddContent(seq++, CreateCell(value?.ToString() ?? "N/A"));
                        }
                        builder.CloseElement(); // Close tr
                    }
                    builder.CloseElement(); // Close tbody
                    builder.CloseElement(); // Close table
                }
            };
        }


        private RenderFragment CreateCell(string text) => (builder) =>
        {
            builder.OpenElement(0, "td");
            builder.AddContent(1, text);
            builder.CloseElement();
        };

    }
}