using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PROG6212_PART2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add in-memory cache services for temporary data storage
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(); // Add session handling

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts(); // Use HSTS for better security
            }

            app.UseHttpsRedirection(); // Ensure HTTPS is used
            app.UseStaticFiles(); // Serve static files from wwwroot

            app.UseRouting(); // Enable routing

            app.UseAuthorization(); // Use authorization middleware

            app.UseSession(); // Enable session handling

            // Default routing configuration
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run(); // Start the application
        }
    }
}
