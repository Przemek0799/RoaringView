using Microsoft.AspNetCore.Components;
using RoaringView.Data;
using RoaringView.Model;
using RoaringView.Service;
using static RoaringView.Data.CompanyInfoService;

namespace RoaringView.Pages
{
    public partial class CompanyInfoPage 
    {
        [Inject]
        private CompanyInfoService ApiService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }
        [Inject]
        private SortingService SortingService { get; set; }
        [Inject]
        private ILogger<CompanyInfoPage> _logger { get; set; }

        private CompanyInfo companyInfo = new CompanyInfo { Companies = new List<Company>() };
        private List<Company> allCompanies = new List<Company>();
        private int currentPage = 1;
        private int itemsPerPage = 25;
        private int totalItems;
        private int totalPages;
        private string currentSortColumn = null;
        private bool sortAscending = true;


        //list for sorting
        private void SortByRoaringCompanyId() => SortData("RoaringCompanyId");
        private void SortByCompanyName() => SortData("CompanyName");
        private void SortByCompanyRegistrationDate() => SortData("CompanyRegistrationDate");
        private void SortByIndustryCode() => SortData("IndustryCode");
        private void SortByIndustryText() => SortData("IndustryText");
        private void SortByLegalGroupCode() => SortData("LegalGroupCode");
        private void SortByLegalGroupText() => SortData("LegalGroupText");
        private void SortByEmployerContributionReg() => SortData("EmployerContributionReg");
        private void SortByNumberEmployeesInterval() => SortData("NumberEmployeesInterval");
        private void SortByPreliminaryTaxReg() => SortData("PreliminaryTaxReg");
        private void SortBySeveralCompanyName() => SortData("SeveralCompanyName");
        private void SortByNumberOfEmployees() => SortData("NumberOfEmployees");

        private async Task SortData(string columnName)
        {
            if (columnName == currentSortColumn)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = columnName;
                sortAscending = true;
            }

            ApplySorting(); // Sort and update the current page
        }
        protected override async Task OnInitializedAsync()
        {
            await FetchAndSortCompanyData();
        }

        private async Task FetchAndSortCompanyData()
        {
            var fetchedCompanies = await ApiService.GetCompanyInfoAsync();
            if (fetchedCompanies == null || fetchedCompanies.Companies == null)
            {
                _logger.LogWarning("No companies or null result returned from ApiService.");
                allCompanies.Clear();
            }
            else
            {
                allCompanies = fetchedCompanies.Companies;
            }

            ApplySorting();
        }

        private void ApplySorting()
        {
            if (!string.IsNullOrEmpty(currentSortColumn))
            {
                allCompanies = SortingService.SortData(allCompanies, currentSortColumn, sortAscending);
            }

            totalItems = allCompanies.Count;
            totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            GoToPage(currentPage); 
        }

        private async Task GoToPage(int page)
        {
            currentPage = page;
            int skip = (currentPage - 1) * itemsPerPage;
            companyInfo.Companies = allCompanies.Skip(skip).Take(itemsPerPage).ToList();
            _logger.LogInformation($"Navigating to page {page}, displaying {companyInfo.Companies.Count} companies.");
            StateHasChanged(); // Notify the component that its state has changed
        }



        public void NavigateToCompany(string roaringCompanyId)
        {
            NavigationManager.NavigateTo($"/Specific-company/{roaringCompanyId}");
        }
        private async Task NextPage()
        {
            _logger.LogInformation($"Navigating to next page: {currentPage + 1}");
            await GoToPage(Math.Min(currentPage + 1, totalPages));
        }

        private async Task PreviousPage() => await GoToPage(Math.Max(currentPage - 1, 1));
        private async Task FirstPage() => await GoToPage(1);
        private async Task LastPage() => await GoToPage(totalPages);

        private bool CanNavigateBackward => currentPage > 1;
        private bool CanNavigateForward => currentPage < totalPages;

       

    }
}
