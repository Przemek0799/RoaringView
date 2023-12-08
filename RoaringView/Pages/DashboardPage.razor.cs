using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using RoaringView.Data;
using RoaringView.Model;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;


namespace RoaringView.Pages
{

    public partial class DashboardPage
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected DashboardService CompanyDataService { get; set; }

        protected Model.FinancialRecord newestFinancialRecord;

        protected List<Model.FinancialRecord> financialRecords; 

        protected List<Model.CompanyRating> specificCompanyRatings;

        [Inject]
        protected ILogger<DashboardPage> Logger { get; set; }

        [Parameter]
        public string RoaringCompanyId { get; set; }

        protected SearchResults companyRelatedData;
        protected bool isLoading = true;
        protected bool shouldRenderChart = false;


        protected override async Task OnInitializedAsync()
        {
            Logger.LogInformation($"Initializing SpecificCompanyPage for Company ID: {RoaringCompanyId}");
            isLoading = true;

            try
            {
                StateHasChanged();
                companyRelatedData = await CompanyDataService.GetCompanySpecificDataAsync(RoaringCompanyId);
                Logger.LogInformation($"Retrieved data: {System.Text.Json.JsonSerializer.Serialize(companyRelatedData)}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while fetching data for Company ID: {0}", RoaringCompanyId);
            }
            finally
            {
                isLoading = false;
            }
            // After fetching data successfully
            if (companyRelatedData?.FinancialRecords != null && companyRelatedData.FinancialRecords.Any())
            {
                newestFinancialRecord = companyRelatedData.FinancialRecords.OrderByDescending(fr => fr.FromDate).FirstOrDefault();

                financialRecords = companyRelatedData.FinancialRecords.OrderBy(fr => fr.FromDate).ToList();

                shouldRenderChart = true;
                StateHasChanged();
            }

        }


        //anropa olika charts, ge dem värde , typ av chart och namn 
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (shouldRenderChart)
            {
                shouldRenderChart = false; 
                try
                {
                    await RenderChart("netOperatingIncomeChart", "Line", fr =>
                        (fr.FromDate.ToString("yyyy-MM-dd"), new[] { fr.PlNetOperatingIncome }, "rgba(0, 123, 255, 0.5)", "rgba(0, 123, 255, 1)")
                    );


                    await RenderChart("ebitdaChart", "Line", fr =>
                        (fr.FromDate.ToString("yyyy-MM-dd"), new[] { fr.KpiEbitda }, "rgba(75, 192, 192, 0.5)", "rgba(75, 192, 192, 1)")
             );

                    await RenderChart("EbitdaMarginPercentChart", "Pie", fr =>
                ($"Year {fr.FromDate.Year}", new[] { fr.KpiEbitdaMarginPercent }, "rgba(0, 123, 255, 0.5)", "rgba(0, 123, 255, 1)")
            );

                    // add more charts
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error occurred while rendering charts.");
                }
            }
        }


     
        private async Task RenderChart(string chartId, string chartType, Func<Model.FinancialRecord, (string Label, IEnumerable<decimal> Data, string BackgroundColor, string BorderColor)> dataSelector)
        {
            try
            {
                var chartOptions = new
                {
                    // You can add specific configuration settings for your chart here
                    // For example: maintainAspectRatio = false, responsive = true, etc.
                };
                if (chartType == "Pie")
                {
                    // Special handling for pie chart
                    var backgroundColors = financialRecords.Select((fr, index) => pieChartColors[index % pieChartColors.Length]).ToArray();
                    var borderColors = backgroundColors; // or use a different set of colors for borders if required

                    var chartData = new
                    {
                        Labels = financialRecords.Select(fr => fr.FromDate.ToString("yyyy-MM-dd")).ToArray(),
                        Datasets = new[]
                        {
                    new
                    {
                        Data = financialRecords.Select(fr => (double)fr.KpiEbitdaMarginPercent).ToArray(),
                        BackgroundColor = backgroundColors,
                        BorderColor = borderColors
                    }
                }
                    };

                    // Invoke JS to render the pie chart
                    await JSRuntime.InvokeVoidAsync($"chartFunctions.create{chartType}Chart", chartId, chartData, chartOptions);
                }
                else
                {
                    // Handling for line and other types of charts
                    var records = financialRecords.Select(fr =>
                    {
                        var data = dataSelector(fr);
                        return (Label: fr.FromDate.ToString("yyyy-MM-dd"), Data: new[] { Convert.ToDecimal(data.Data.FirstOrDefault()) }, data.BackgroundColor, data.BorderColor);
                    }).ToArray();

                    var chartData = new
                    {
                        Labels = records.Select(r => r.Label).ToArray(),
                        Datasets = new[]
                        {
                    new
                    {
                        Data = records.SelectMany(r => r.Data.Select(d => (double)d)).ToArray(),
                        BackgroundColor = records.Select(r => r.BackgroundColor).ToArray(),
                        BorderColor = records.Select(r => r.BorderColor).ToArray()
                    }
                }
                    };

                    // Invoke JS to render line or other types of charts
                    await JSRuntime.InvokeVoidAsync($"chartFunctions.create{chartType}Chart", chartId, chartData, chartOptions);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error occurred while rendering {chartType} chart ('{chartId}').");
            }
        }



        //färger för pie chart
        private readonly string[] pieChartColors = new[]
        {
            "rgba(255, 99, 132, 0.5)",  // Red
            "rgba(54, 162, 235, 0.5)",  // Blue
            "rgba(255, 206, 86, 0.5)",  // Yellow
            "rgba(75, 192, 192, 0.5)",  // Green
            "rgba(153, 102, 255, 0.5)", // Purple
            // Add more colors as needed
        };


    }
}