using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using RoaringView.Data;
using RoaringView.Model;
using RoaringView.Service;
using System;
using System.Threading.Tasks;

namespace RoaringView.Pages
{
    public partial class CompanyInfoPage 
    {
        [Inject]
        private CompanyInfoService ApiService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }
        [Inject]
       
        AuthenticationStateProvider AuthenticationStateProvider { get; set; } 
        [Inject]
        private SortingService SortingService { get; set; }
        [Inject]
        private IHttpContextAccessor HttpContextAccessor { get; set; }


        private CompanyInfo companyInfo;

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
        private void SortByNumberCompanyUnits() => SortData("NumberCompanyUnits");
        private void SortByNumberEmployeesInterval() => SortData("NumberEmployeesInterval");
        private void SortByPreliminaryTaxReg() => SortData("PreliminaryTaxReg");
        private void SortBySeveralCompanyName() => SortData("SeveralCompanyName");
        private void SortByNumberOfEmployees() => SortData("NumberOfEmployees");


        protected override async Task OnInitializedAsync()
        {
            companyInfo = await ApiService.GetCompanyInfoAsync();
        }

        public void NavigateToCompany(string roaringCompanyId)
        {
            NavigationManager.NavigateTo($"/Specific-company/{roaringCompanyId}");
        }

        private async Task SortData(string columnName)
        {
            if (companyInfo?.Companies == null) return;

            if (columnName == currentSortColumn)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = columnName;
                sortAscending = true;
            }

            companyInfo.Companies = SortingService.SortData(companyInfo.Companies, columnName, sortAscending);
        }






    }
}
