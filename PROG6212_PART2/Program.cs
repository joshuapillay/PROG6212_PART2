using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace PROG6212_PART2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ClaimsDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ClaimsDBContext")));

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false; // Set to true if you want email confirmation
            })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ClaimsDbContext>();

            // Add in-memory cache services for temporary data storage
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            var app = builder.Build();

            // Seed data after the application has started
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await SeedData.Initialize(services); // Call the seed method
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding data: {ex.Message}");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication(); // Add authentication middleware
            app.UseAuthorization();
            app.UseSession();
            app.MapRazorPages();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
