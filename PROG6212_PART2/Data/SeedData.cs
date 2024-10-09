using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "Lecturer", "Coordinator", "Manager" };
        IdentityResult roleResult;

        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Seed an admin user with an email address
        var adminEmail = "admin@example.com";
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
        };

        var user = await userManager.FindByEmailAsync(adminEmail);

        if (user == null)
        {
            var createPowerUser = await userManager.CreateAsync(adminUser, "Admin@123");
            if (createPowerUser.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Manager");
            }
        }



        var lecturerEmail = "lecturer@example.com";
        var lecturerUser = new ApplicationUser
        {
            UserName = lecturerEmail, 
            Email = lecturerEmail, 
        };

        var existingLecturer = await userManager.FindByEmailAsync(lecturerEmail);

        if (existingLecturer == null)
        {
            var createLecturer = await userManager.CreateAsync(lecturerUser, "Lecturer@123");
            if (createLecturer.Succeeded)
            {
                await userManager.AddToRoleAsync(lecturerUser, "Lecturer");
            }
        }

    }
}
