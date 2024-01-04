using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RoaringView.Data;
using RoaringView.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RoaringView.Model.Identity;
using RoaringView.Pages.Identity;
using Microsoft.AspNetCore.Http.Connections;
using Serilog.Formatting.Compact;

namespace RoaringView
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                 .WriteTo.File(new CompactJsonFormatter(), "Logs/logs-.txt",
            rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7) // Keep log files for 7 days
               .CreateLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add Serilog
                builder.Host.UseSerilog();

                // Add services to the container.
                builder.Services.AddRazorPages();
                builder.Services.AddServerSideBlazor();

                // Adds DbContext and Identity
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

                // for Services inside Data folder
                builder.Services.AddHttpClient<CompanyStructureService>();
                builder.Services.AddHttpClient<CompanyInfoService>();
                builder.Services.AddHttpClient<FilteredSearchService>();
                builder.Services.AddHttpClient<GeneralSearchService>();
                builder.Services.AddHttpClient<DashboardService>();
                builder.Services.AddHttpClient<CompanyHierarchyService>();
                builder.Services.AddScoped<CompanySearchService>();


                builder.Services.AddScoped<SortingService>();
                builder.Services.AddLogging();

                builder.Services.AddHttpContextAccessor();


                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error");
                }


                app.UseStaticFiles();

                app.UseRouting();

                //for identity/login/register
                app.UseAuthentication();
                app.UseAuthorization();

                // Retrieve JWT Key and configure middleware to fix the blazor http header error
                var RoaringjwtKey = builder.Configuration["RoaringjwtKey"];
                app.UseMiddleware<BlazorCookieLoginMiddleware>(RoaringjwtKey);


                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapBlazorHub(configureOptions: options =>
                    {
                        options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
                    });
                    endpoints.MapFallbackToPage("/_Host");
                    endpoints.MapRazorPages();
                });

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}