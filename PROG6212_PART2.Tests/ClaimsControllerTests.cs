using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ClaimsControllerTests
{
    private ClaimsDbContext GetInMemoryDbContext()
    {
        // Set up an in-memory database for unit testing
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        var context = new ClaimsDbContext(options);

        context.Database.EnsureDeleted(); // Ensure fresh state before each test
        context.Database.EnsureCreated();

        return context;
    }

    [Fact]
    public async Task SubmitClaim_ValidModel_ReturnsViewResult()
    {
        // Arrange: Create an in-memory context and controller
        var context = GetInMemoryDbContext();
        var controller = new ClaimsController(context);

        // Act: Submit a valid claim
        var claim = new Claim
        {
            LecturerName = "Test Lecturer",
            HoursWorked = 10,
            HourlyRate = 50,
            Notes = "Test Notes"
        };

        var result = await controller.SubmitClaim(claim, null);

        // Assert: Ensure the result is a view
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(string.IsNullOrEmpty(viewResult.ViewName) || viewResult.ViewName == "SubmitClaim");
    }

    [Fact]
    public async Task ViewPendingClaims_ReturnsPendingClaims()
    {
        var context = GetInMemoryDbContext();
        var controller = new ClaimsController(context);

        // Arrange: Add some pending claims
        var pendingClaim1 = new Claim { LecturerName = "Lecturer1", Status = "Pending", DocumentPath = "/uploads/doc1.pdf", Notes = "Notes1" };
        var pendingClaim2 = new Claim { LecturerName = "Lecturer2", Status = "Pending", DocumentPath = "/uploads/doc2.pdf", Notes = "Notes2" };
        context.Claims.AddRange(pendingClaim1, pendingClaim2);
        await context.SaveChangesAsync();

        // Act: Retrieve pending claims
        var result = await controller.ViewPendingClaims();

        // Assert: Check that the correct number of claims is returned
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Claim>>(viewResult.ViewData.Model);
        Assert.Equal(2, model.Count);
    }

    [Fact]
    public async Task ApproveClaim_ChangesStatusToApproved()
    {
        var context = GetInMemoryDbContext();
        var controller = new ClaimsController(context);

        // Arrange: Add a pending claim
        var claim = new Claim { LecturerName = "Lecturer", Status = "Pending", DocumentPath = "/uploads/doc.pdf", Notes = "Notes" };
        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        // Act: Approve the claim
        await controller.ApproveClaim(claim.Id);

        // Assert: Ensure the claim status is updated to "Approved"
        var updatedClaim = await context.Claims.FindAsync(claim.Id);
        Assert.Equal("Approved", updatedClaim.Status);
    }

    [Fact]
    public async Task RejectClaim_ChangesStatusToRejected()
    {
        var context = GetInMemoryDbContext();
        var controller = new ClaimsController(context);

        // Arrange: Add a pending claim
        var claim = new Claim { LecturerName = "Lecturer", Status = "Pending", DocumentPath = "/uploads/doc.pdf", Notes = "Notes" };
        context.Claims.Add(claim);
        await context.SaveChangesAsync();

        // Act: Reject the claim
        await controller.RejectClaim(claim.Id);

        // Assert: Ensure the claim status is updated to "Rejected"
        var updatedClaim = await context.Claims.FindAsync(claim.Id);
        Assert.Equal("Rejected", updatedClaim.Status);
    }

    [Fact]
    public async Task TrackClaims_ReturnsAllClaims()
    {
        var context = GetInMemoryDbContext();
        var controller = new ClaimsController(context);

        // Arrange: Add multiple claims with different statuses
        var claim1 = new Claim { LecturerName = "Lecturer1", Status = "Approved", DocumentPath = "/uploads/doc1.pdf", Notes = "Notes1" };
        var claim2 = new Claim { LecturerName = "Lecturer2", Status = "Rejected", DocumentPath = "/uploads/doc2.pdf", Notes = "Notes2" };
        context.Claims.AddRange(claim1, claim2);
        await context.SaveChangesAsync();

        // Act: Track all claims
        var result = await controller.TrackClaims();

        // Assert: Ensure all claims are retrieved
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Claim>>(viewResult.ViewData.Model);
        Assert.Equal(2, model.Count);
    }
    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]

}
