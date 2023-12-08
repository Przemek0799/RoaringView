//using Microsoft.AspNetCore.Components;
//using RoaringView.Data;
//using RoaringView.Model;
//using RoaringView.Pages;

//public partial class CompanyHierarchyPage : ComponentBase
//{
//    [Inject]
//    protected CompanyDataService CompanyDataService { get; set; }

//    [Inject]
//    protected ILogger<CompanyDataService> Logger { get; set; }

//    [Parameter]
//    public string RoaringCompanyId { get; set; }

//    protected SearchResults companyRelatedData;

//    protected override async Task OnInitializedAsync()
//    {
//        Logger.LogInformation($"Initializing SpecificCompanyPage for Company ID: {RoaringCompanyId}");
        
//        try
//        {
//            companyRelatedData = await CompanyDataService.GetCompanySpecificDataAsync(RoaringCompanyId);
//            Logger.LogInformation($"Retrieved data: {System.Text.Json.JsonSerializer.Serialize(companyRelatedData)}");
//        }
//        catch (Exception ex)
//        {
//            Logger.LogError(ex, "Error occurred while fetching data for Company ID: {0}", RoaringCompanyId);
//        }
       
//    }
//}
