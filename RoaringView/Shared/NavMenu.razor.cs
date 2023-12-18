using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RoaringView.Data;
using RoaringView.Pages;
using System;
using System.Globalization;

namespace RoaringView.Shared
{
    public partial class NavMenu
    {
        [Inject]
        public GeneralSearchService SearchService { get; set; }
        [Inject]
        public CompanyInfoService CompanyInfoService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        [Inject]
        private ILogger<NavMenu> Logger { get; set; }


        private string generalSearchTerm = "";
        private string companySearchTerm = "";
        private string filterCompanyName = "";
        private string filterRoaringCompanyId = "";
        private DateTime? filterStartDate;
        private DateTime? filterEndDate;
        private int? minRating;
        private int? maxRating;

        //changing date format to dd//mm//yyyy
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("initializeFlatpickr", "startDate", DotNetObjectReference.Create(this), "SetStartDate");
                await JSRuntime.InvokeVoidAsync("initializeFlatpickr", "endDate", DotNetObjectReference.Create(this), "SetEndDate");
            }
        }

        [JSInvokable]
        public void SetStartDate(string dateStr)
        {
            if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                filterStartDate = date;
            }
            else
            {
                filterStartDate = null;
            }
        }

        [JSInvokable]
        public void SetEndDate(string dateStr)
        {
            if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                filterEndDate = date;
            }
            else
            {
                filterEndDate = null;
            }
        }


        private void PerformGeneralSearch()
        {
            NavigationManager.NavigateTo($"/search-results/{generalSearchTerm}");
        }


        private void PerformSpecificCompanySearch()
        {
            // Navigate to CompanyDetailsPage with the company ID
            NavigationManager.NavigateTo($"/Specific-company/{companySearchTerm}");
        }

        private void PerformOrgStructureSearch()
        {
            // Navigate to CompanyDetailsPage with the company ID
            NavigationManager.NavigateTo($"/Company-Hierarchy/{companySearchTerm}");
        }

        private void NavigateToAddCompanyPage()
        {
            NavigationManager.NavigateTo("AddCompany");
        }

        private void PerformFilteredSearch()
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(filterCompanyName))
                queryParams.Add($"companyName={filterCompanyName}");
            if (!string.IsNullOrWhiteSpace(filterRoaringCompanyId))
                queryParams.Add($"roaringCompanyId={filterRoaringCompanyId}");
            if (filterStartDate.HasValue)
                queryParams.Add($"startDate={filterStartDate.Value.ToString("yyyy-MM-dd")}");
            if (filterEndDate.HasValue)
                queryParams.Add($"endDate={filterEndDate.Value.ToString("yyyy-MM-dd")}");
            if (minRating.HasValue)
                queryParams.Add($"minRating={minRating}");
            if (maxRating.HasValue)
                queryParams.Add($"maxRating={maxRating}");

            var queryString = string.Join("&", queryParams);
            NavigationManager.NavigateTo($"/filtered-search-results?{queryString}");
        }
    }
}

