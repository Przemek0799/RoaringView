﻿using Microsoft.AspNetCore.Components;
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

        public string Town { get; set; }
        public string CompanyName { get; set; }
        public string ZipCode { get; set; }
        public string IndustryCode { get; set; }
        public string LegalGroupText { get; set; }
        public string NumberEmployeesInterval { get; set; }
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
            Logger.LogInformation($"Search results: {SearchResult}");
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
                Logger.LogInformation($"Company data saved for ID: {companyId}");
                // Optionally, refresh the page or display a success message
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error occurred while saving company data for ID: {companyId}");
            }
        }
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
                if (SearchResult?.Hits != null && SearchResult.Hits.Any())
                {
                    Logger.LogInformation($"Building table for RoaringSearchResponse with {SearchResult.Hits.Count()} items.");
                    int seq = 0;
                    builder.OpenElement(seq++, "table");
                    builder.AddAttribute(seq++, "class", "table");

                    // Table Header with Sortable Columns and Save All Button
                    builder.OpenElement(seq++, "thead");
                    builder.OpenElement(seq++, "tr");

                    foreach (var header in new[] { "CompanyName", "LegalGroupCode", "Town", "LegalGroupText" })
                    {
                        builder.OpenElement(seq++, "th");
                        if (onHeaderClick != null)
                        {
                            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => onHeaderClick(header)));
                        }
                        builder.AddContent(seq++, header);
                        builder.CloseElement(); // Close th
                    }

                    // Save All Button in the header row
                    builder.OpenElement(seq++, "th");
                    builder.OpenElement(seq++, "button");
                    builder.AddAttribute(seq++, "class", "btn btn-success");
                    builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(this, SaveAllCompanies));
                    builder.AddContent(seq++, "Save All");
                    builder.CloseElement(); // Close button
                    builder.CloseElement(); // Close th

                    builder.CloseElement(); // Close tr
                    builder.CloseElement(); // Close thead

                    // Table Body
                    builder.OpenElement(seq++, "tbody");
                    foreach (var item in SearchResult.Hits)
                    {
                        builder.OpenElement(seq++, "tr");

                        var values = new object[] { item.CompanyName, item.LegalGroupCode, item.Town, item.LegalGroupText };
                        foreach (var value in values)
                        {
                            builder.OpenElement(seq++, "td");
                            builder.AddContent(seq++, value?.ToString() ?? "N/A");
                            builder.CloseElement(); // Close td
                        }

                        // Individual Save button for each row
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