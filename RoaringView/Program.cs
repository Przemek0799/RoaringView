using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RoaringView.Data;
using RoaringView.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RoaringView.Model.Identity;
using RoaringView.Pages.Identity;

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
            //middleware to fix blazor login problem
            app.UseMiddleware<BlazorCookieLoginMiddleware>();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapRazorPages();
            });

            app.Run();
        }
    }
}