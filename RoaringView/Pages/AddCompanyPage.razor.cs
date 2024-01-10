using Microsoft.AspNetCore.Components;
using RoaringView.Data;
using RoaringView.Model;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Rendering;
using RoaringView.Service;

namespace RoaringView.Pages
{
    public partial class AddCompanyPage
    {
        [Inject]
        public CompanySearchService CompanySearchService { get; set; }

        [Inject]
        private ILogger<AddCompanyPage> _logger { get; set; }
        public RoaringSearchResult SearchResult { get; set; }
        [Inject]
        NavigationManager NavigationManager { get; set; }

        private string currentSortColumn = null;
        private bool sortAscending = true;
        private bool isAdvancedSearch = false; // This variable tracks the search mode

        public string Town { get; set; }
        public string CompanyName { get; set; }
        public string ZipCode { get; set; }
        public string IndustryCode { get; set; }
        public string LegalGroupText { get; set; }
        public string NumberEmployeesInterval { get; set; }
        public string FreeText { get; set; }

        private List<Company> allCompanies = new List<Company>(); // Full list of companies
        private int currentPage = 1;
        private int itemsPerPage = 20;
        private int totalItems;
        private int totalPages;
        private bool CanNavigateForward => currentPage < totalPages;

        private bool CanNavigateBackward => currentPage > 1;

        private async Task SearchCompany()
        {
            var searchParams = new Dictionary<string, string>();
            if (isAdvancedSearch)
            {
                AddToDictionaryIfNotEmpty(searchParams, "companyName", CompanyName);
                AddToDictionaryIfNotEmpty(searchParams, "town", Town);
                AddToDictionaryIfNotEmpty(searchParams, "zipCode", ZipCode);
                AddToDictionaryIfNotEmpty(searchParams, "industryCode", IndustryCode);
                AddToDictionaryIfNotEmpty(searchParams, "legalGroupText", LegalGroupText);
                AddToDictionaryIfNotEmpty(searchParams, "numberEmployeesInterval", NumberEmployeesInterval);
            }
            else
            {
                AddToDictionaryIfNotEmpty(searchParams, "freeText", FreeText);
            }

            SearchResult = await CompanySearchService.SearchAsync(searchParams);
            _logger.LogInformation($"Search results: {SearchResult}");
        }
        private void ToggleSearchMode()
        {
            isAdvancedSearch = !isAdvancedSearch; // Toggle the search mode
        }

        //allows to not fill all inputs
        private void AddToDictionaryIfNotEmpty(Dictionary<string, string> dict, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                dict[key] = value;
            }
        }

        // Add a method to handle the save action
        private async Task SaveCompany(string companyId)
        {
            try
            {
                await CompanySearchService.SaveCompanyDataAsync(companyId);
                _logger.LogInformation($"Company data saved for ID: {companyId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while saving company data for ID: {companyId}");
            }
        }

        //navigate to dashboard from specific companyname in data table
        private async Task NavigateToCompany(string companyId)
        {
            try
            {
                var roaringCompanyId = await CompanySearchService.SaveCompanyDataAsync(companyId);
                _logger.LogInformation($"Navigating to company dashboard for RoaringCompanyId: {roaringCompanyId}");
                NavigationManager.NavigateTo($"/Specific-company/{roaringCompanyId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while navigating to company dashboard for ID: {companyId}");
            }
        }

        //function to loop through savecompany function and save all companies
        private async Task SaveAllCompanies()
        {
            if (SearchResult?.Hits != null)
            {
                foreach (var company in SearchResult.Hits)
                {
                    await SaveCompany(company.CompanyId);
                }
            }
        }


        private void GoToPage(int page)
        {
            currentPage = page;
            //ApplyPagination();
        }

        private void NextPage() => GoToPage(Math.Min(currentPage + 1, totalPages));
        private void PreviousPage() => GoToPage(Math.Max(currentPage - 1, 1));
        private void FirstPage() => GoToPage(1);
        private void LastPage() => GoToPage(totalPages);
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

        public RenderFragment DisplayCompaniesTable => builder =>
        {
            if (SearchResult?.Hits != null && SearchResult.Hits.Any())
            {
                BuildTableFragment(builder, SearchResult.Hits,
                    new[] { "CompanyName", "LegalGroupCode", "Town", "LegalGroupText" },
                    company => new object[] { company.CompanyName, company.LegalGroupCode, company.Town, company.LegalGroupText },
                    company => $"/Specific-company/{company.CompanyId}",
                    columnName => SortData(columnName));
            }
            else
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "alert alert-info");
                builder.AddContent(2, "No companies found matching the search criteria.");
                builder.CloseElement();
            }
        };
        private void BuildTableFragment(RenderTreeBuilder builder, IEnumerable<RoaringSearchResponse> items, string[] headers, Func<RoaringSearchResponse, object[]> valueSelector, Func<RoaringSearchResponse, string> navigateUrlSelector, Action<string> onHeaderClick)
        {
            if (items != null && items.Any())
            {
                _logger.LogInformation($"Building table for RoaringSearchResponse with {items.Count()} items.");
                int seq = 0;
                builder.OpenElement(seq++, "table");
                builder.AddAttribute(seq++, "class", "table");

                // Table Header with Sortable Columns and Save All Button
                BuildTableHeaderOrFooter(builder, ref seq, headers, onHeaderClick, true, true);

                // Table Body
                BuildTableBody(builder, ref seq, items, headers, valueSelector, navigateUrlSelector);

                // Table Footer (mirroring the Header) with Save All Button
                BuildTableHeaderOrFooter(builder, ref seq, headers, onHeaderClick, false, true);

                builder.CloseElement(); // Close table
            }
            else
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "alert alert-info");
                builder.AddContent(2, "No companies found matching the search criteria.");
                builder.CloseElement();
            }
        }

        private void BuildTableHeaderOrFooter(RenderTreeBuilder builder, ref int seq, string[] headers, Action<string> onHeaderClick, bool isHeader, bool addSaveAllButton)
        {
            var tag = isHeader ? "thead" : "tfoot";
            builder.OpenElement(seq++, tag);
            builder.OpenElement(seq++, "tr");

            foreach (var header in headers)
            {
                builder.OpenElement(seq++, "th");
                builder.AddAttribute(seq++, "class", "header-cell"); // Apply the class here
                if (onHeaderClick != null)
                {
                    // Apply onclick for sorting on both header and footer
                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => onHeaderClick(header)));
                }
                builder.AddContent(seq++, header);
                builder.CloseElement(); // Close th
            }

            // Add Save All Button in the header/footer row
            builder.OpenElement(seq++, "th");
            if (addSaveAllButton)
            {
                builder.OpenElement(seq++, "button");
                builder.AddAttribute(seq++, "class", "btn btn-success");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, SaveAllCompanies));
                builder.AddContent(seq++, "Save All");
                builder.CloseElement(); // Close button
            }
            builder.CloseElement(); // Close th

            builder.CloseElement(); // Close tr
            builder.CloseElement(); // Close tag (thead or tfoot)
        }



        private void BuildTableBody(RenderTreeBuilder builder, ref int seq, IEnumerable<RoaringSearchResponse> items, string[] headers, Func<RoaringSearchResponse, object[]> valueSelector, Func<RoaringSearchResponse, string> navigateUrlSelector)
        {
            builder.OpenElement(seq++, "tbody");
            foreach (var item in items)
            {
                builder.OpenElement(seq++, "tr");

                var values = valueSelector(item);
                for (int i = 0; i < values.Length; i++)
                {
                    builder.OpenElement(seq++, "td");
                    if (headers[i] == "CompanyName")
                    {
                        builder.AddAttribute(seq++, "class", "clickable-cell");
                        builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => NavigateToCompany(item.CompanyId)));
                    }
                    builder.AddContent(seq++, values[i]?.ToString() ?? "N/A");
                    builder.CloseElement(); // Close 'td'
                }

                // Individual Save button for each row
                builder.OpenElement(seq++, "td");
                builder.OpenElement(seq++, "button");
                builder.AddAttribute(seq++, "class", "btn custom-button-color");
                builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, () => SaveCompany(item.CompanyId)));
                builder.AddContent(seq++, "Save");
                builder.CloseElement(); // Close button
                builder.CloseElement(); // Close td

                builder.CloseElement(); // Close tr
            }
            builder.CloseElement(); // Close tbody
        }

    }
}