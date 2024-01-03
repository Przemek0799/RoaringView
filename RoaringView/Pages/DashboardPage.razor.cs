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
        protected ILogger<DashboardPage> _logger { get; set; }

        [Parameter]
        public string RoaringCompanyId { get; set; }

        protected SearchResults companyRelatedData;
        protected bool isLoading = true;
        protected bool shouldRenderChart = false;
        private string errorMessage;
        private string previousRoaringCompanyId;
        private bool isDataLoaded;
        private bool isFinancialDataFetched = false;


        // OnParametersSetAsync and LoadData updates page if a new search was made and url changed

        private async Task LoadData()
        {
            try
            {
                companyRelatedData = await CompanyDataService.GetCompanySpecificDataAsync(RoaringCompanyId);
                _logger.LogInformation($"Retrieved data: {System.Text.Json.JsonSerializer.Serialize(companyRelatedData)}");
                // Other logic to handle the loaded data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching data for Company ID: {0}", RoaringCompanyId);
                // Error handling logic
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

       protected override async Task OnInitializedAsync()
{
            _logger.LogInformation($"Initializing DashboardPage for Company ID: {RoaringCompanyId}");
    isLoading = true;
            try
            {
                companyRelatedData = await CompanyDataService.GetCompanySpecificDataAsync(RoaringCompanyId);
                _logger.LogInformation($"Retrieved data: {System.Text.Json.JsonSerializer.Serialize(companyRelatedData)}");

                if (companyRelatedData?.FinancialRecords != null && companyRelatedData.FinancialRecords.Any())
                {
                    newestFinancialRecord = companyRelatedData.FinancialRecords.OrderByDescending(fr => fr.FromDate).FirstOrDefault();
                    financialRecords = companyRelatedData.FinancialRecords.OrderBy(fr => fr.FromDate).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching data for Company ID: {0}", RoaringCompanyId);
                errorMessage = "Failed to load data.";
            }
            finally
            {
                isLoading = false;
                isDataLoaded = true; // Set the flag indicating that data is loaded
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (RoaringCompanyId != previousRoaringCompanyId)
            {
                previousRoaringCompanyId = RoaringCompanyId;
                _logger.LogInformation($"Parameter changed, new Company ID: {RoaringCompanyId}");
                await LoadData();
            }

            if (isDataLoaded)
            {
                await RenderCharts(); // Call the method to render charts
            }
        }
        private async Task RenderCharts()
        {
            try
            {
                // Render the Net Operating Income Line Chart
                await LineRenderChart("netOperatingIncomeChart", fr =>
                    (fr.FromDate.ToString("yyyy-MM-dd"), new[] { fr.PlNetOperatingIncome }, "rgba(0, 123, 255, 0.5)", "rgba(0, 123, 255, 1)")
                );

                // Render the EBITDA Line Chart
                await LineRenderChart("ebitdaChart", fr =>
                    (fr.FromDate.ToString("yyyy-MM-dd"), new[] { fr.KpiEbitda }, "rgba(75, 192, 192, 0.5)", "rgba(75, 192, 192, 1)")
                );

                // Render the Ebitda Margin Percent Pie Chart
                await PieRenderChart("EbitdaMarginPercentChart", fr =>
                    ($"Year {fr.FromDate.Year}", new[] { fr.KpiEbitdaMarginPercent }, "rgba(0, 123, 255, 0.5)", "rgba(0, 123, 255, 1)")
                );

                // Add additional calls to render other charts as needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while rendering charts.");
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
                _logger.LogError(ex, $"Error occurred while rendering Pie chart ('{chartId}').");
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
                _logger.LogError(ex, $"Error occurred while rendering Line chart ('{chartId}').");
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
                isFinancialDataFetched = true;

                // Force a page refresh
                await JSRuntime.InvokeVoidAsync("location.reload");
                // maybe in future try to update only 
            }
            catch (HttpRequestException ex)
            {
                SetErrorMessage("No financial data available to fetch for this company.", ex);
            }
            catch (Exception ex)
            {
                SetErrorMessage("An error occurred while processing your request.", ex);
            }
        }



        private async Task FetchCompanyInfoFromRoaring()
        {
            try
            {
                await CompanyDataService.FetchAndSaveCompanyInfo(RoaringCompanyId);
                await OnInitializedAsync(); // Refresh the dashboard
            }
            catch (HttpRequestException ex)
            {
                SetErrorMessage("No company information available to fetch for this company.", ex);
            }
            catch (Exception ex)
            {
                SetErrorMessage("An error occurred while processing your request.", ex);
            }
        }

        private async Task FetchCompanyRatingFromRoaring()
        {
            try
            {
                await CompanyDataService.FetchAndSaveCompanyRating(RoaringCompanyId);
                await OnInitializedAsync(); // Refresh the dashboard
            }
            catch (HttpRequestException ex)
            {
                SetErrorMessage("No company ratings available to fetch for this company.", ex);
            }
            catch (Exception ex)
            {
                SetErrorMessage("An error occurred while processing your request.", ex);
            }
        }

        private void SetErrorMessage(string message, Exception ex)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = message;
                _logger.LogError(ex, message);
            }
        }




    }
}