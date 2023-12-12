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
        private string errorMessage;



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
                    await LineRenderChart("netOperatingIncomeChart", fr =>
                        (fr.FromDate.ToString("yyyy-MM-dd"), new[] { fr.PlNetOperatingIncome }, "rgba(0, 123, 255, 0.5)", "rgba(0, 123, 255, 1)")
                    );

                    await LineRenderChart("ebitdaChart", fr =>
                        (fr.FromDate.ToString("yyyy-MM-dd"), new[] { fr.KpiEbitda }, "rgba(75, 192, 192, 0.5)", "rgba(75, 192, 192, 1)")
                    );

                    await PieRenderChart("EbitdaMarginPercentChart", fr =>
                        ($"Year {fr.FromDate.Year}", new[] { fr.KpiEbitdaMarginPercent }, "rgba(0, 123, 255, 0.5)", "rgba(0, 123, 255, 1)")
                    );

                    // Add more charts if needed
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error occurred while rendering charts.");
                }
            }
        }



        private async Task PieRenderChart(string chartId, Func<Model.FinancialRecord, (string Label, IEnumerable<decimal> Data, string BackgroundColor, string BorderColor)> dataSelector)
        {
            try
            {
                var chartOptions = new
                {
                    // Configuration settings specific to pie chart
                };

                var backgroundColors = financialRecords.Select((fr, index) => pieChartColors[index % pieChartColors.Length]).ToArray();
                var borderColors = backgroundColors; // Or use different colors for borders

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
                await JSRuntime.InvokeVoidAsync($"chartFunctions.createPieChart", chartId, chartData, chartOptions);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error occurred while rendering Pie chart ('{chartId}').");
            }
        }

        private async Task LineRenderChart(string chartId, Func<Model.FinancialRecord, (string Label, IEnumerable<decimal> Data, string BackgroundColor, string BorderColor)> dataSelector)
        {
            try
            {
                var chartOptions = new
                {
                    // Configuration settings specific to line chart
                };

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

                // Invoke JS to render the line chart
                await JSRuntime.InvokeVoidAsync($"chartFunctions.createLineChart", chartId, chartData, chartOptions);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error occurred while rendering Line chart ('{chartId}').");
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


        // fetch buttons to fetch data from roaring
        private async Task FetchFinancialRecordsFromRoaring()
        {
            try
            {
                await CompanyDataService.FetchAndSaveFinancialRecords(RoaringCompanyId);
                await OnInitializedAsync(); // Reload the data
            }
            catch (HttpRequestException ex)
            {
                errorMessage = "No financial data available to fetch for this company.";
                Logger.LogError(ex, "Error occurred while fetching financial records from Roaring.");
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred while processing your request.";
                Logger.LogError(ex, "Error occurred while fetching financial records from Roaring.");
            }
        }

        private async Task FetchCompanyInfoFromRoaring()
        {
            try
            {
                await CompanyDataService.FetchAndSaveCompanyInfo(RoaringCompanyId);
               
                // After fetching and saving data, refresh the dashboard
                await OnInitializedAsync();
            }
            catch (HttpRequestException ex)
            {
                errorMessage = "No financial data available to fetch for this company.";
                Logger.LogError(ex, "Error occurred while fetching company data from Roaring.");
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred while processing your request.";
                Logger.LogError(ex, "Error occurred while fetching company data from Roaring.");
            }
        }

        private async Task FetchCompanyRatingFromRoaring()
        {
            try
            {
                // Use the injected service instance to call the method
                await CompanyDataService.FetchAndSaveCompanyRating(RoaringCompanyId);

                // After fetching and saving data, refresh the dashboard
                await OnInitializedAsync();
            }
            catch (HttpRequestException ex)
            {
                errorMessage = "No company ratings available to fetch for this company.";
                Logger.LogError(ex, "Error occurred while fetching financial records from Roaring.");
            }
            catch (Exception ex)
            {
                errorMessage = "An error occurred while processing your request.";
                Logger.LogError(ex, "Error occurred while fetching company ratings from Roaring.");
            }
        }


    }
}