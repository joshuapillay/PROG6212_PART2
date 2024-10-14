using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

public class SeedData
{
    // Method to initialize roles and users in the system
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Define the roles that need to be created
        string[] roleNames = { "Lecturer", "Coordinator", "Manager" };
        IdentityResult roleResult;

        // Loop through each role and create if it doesn't exist
        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create the manager user if it doesn't exist
        var adminEmail = "Manager@example.com";
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
        };

        var user = await userManager.FindByEmailAsync(adminEmail);

        if (user == null)
        {
            // Create the manager user and assign them the Manager role
            var createPowerUser = await userManager.CreateAsync(adminUser, "Manager@123");
            if (createPowerUser.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Manager");
            }
        }

        // Create the lecturer user if it doesn't exist
        var lecturerEmail = "lecturer@example.com";
        var lecturerUser = new ApplicationUser
        {
            UserName = lecturerEmail,
            Email = lecturerEmail,
        };

        var existingLecturer = await userManager.FindByEmailAsync(lecturerEmail);

        if (existingLecturer == null)
        {
            // Create the lecturer user and assign them the Lecturer role
            var createLecturer = await userManager.CreateAsync(lecturerUser, "Lecturer@123");
            if (createLecturer.Succeeded)
            {
                await userManager.AddToRoleAsync(lecturerUser, "Lecturer");
            }
        }

        // Create the coordinator user if it doesn't exist
        var coordinatorEmail = "coordinator@example.com";
        var coordinatorUser = new ApplicationUser
        {
            UserName = coordinatorEmail,
            Email = coordinatorEmail,
        };

        var existingCoordinator = await userManager.FindByEmailAsync(coordinatorEmail);

        if (existingCoordinator == null)
        {
            // Create the coordinator user and assign them the Coordinator role
            var createCoordinator = await userManager.CreateAsync(coordinatorUser, "Coordinator@123");
            if (createCoordinator.Succeeded)
            {
                await userManager.AddToRoleAsync(coordinatorUser, "Coordinator");
            }
        }
    }
    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]

}
