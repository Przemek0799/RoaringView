using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RoaringView.Data;
using RoaringView.Service;

namespace RoaringView
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
      
            builder.Services.AddHttpClient<CompanyStructureService>();
            builder.Services.AddHttpClient<FinancialRecordService>();
            builder.Services.AddHttpClient<CompanyRatingService>();
            builder.Services.AddHttpClient<CompanyInfoService>();
            builder.Services.AddHttpClient<FilteredSearchService>();
            builder.Services.AddHttpClient<GeneralSearchService>();
            builder.Services.AddHttpClient<DashboardService>();


            builder.Services.AddLogging();
            builder.Services.AddScoped<SortingService>();






            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }


            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}