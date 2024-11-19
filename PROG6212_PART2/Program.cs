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

            builder.Services.AddControllersWithViews();

            // Configure the DbContext to use SQL Server and the connection string from configuration
            builder.Services.AddDbContext<ClaimsDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ClaimsDBContext")));

            // Add Identity services for user authentication and authorization, including roles
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation for login
                options.SignIn.RequireConfirmedEmail = false; // Ensure email confirmation is not required
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ClaimsDbContext>(); // Use the ClaimsDbContext for identity storage

            // Add distributed memory cache and session services
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            var app = builder.Build();

            // Seed the database with initial data (roles and users)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await SeedData.Initialize(services); // Seed the roles and users
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding data: {ex.Message}");
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Enable HTTPS, static files, routing, and session
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession(); // Enable session management

            // Map Razor Pages for Identity
            app.MapRazorPages();

            // Define the default routing pattern
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]
}
