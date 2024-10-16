using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class ClaimsController : Controller
{
    private readonly ClaimsDbContext _context;
    private readonly long _maxFileSize = 5 * 1024 * 1024; // Maximum file size of 5 MB
    private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx" }; // Allowed file extensions

    // Constructor to initialize the database context
    public ClaimsController(ClaimsDbContext context)
    {
        _context = context;
    }

    // GET method for displaying the Submit Claim form
    [Authorize(Roles = "Lecturer")]
    [HttpGet]
    public IActionResult SubmitClaim()
    {
        return View();
    }

    // POST method to handle claim submission with file upload
    [Authorize(Roles = "Lecturer")]
    [HttpPost]
    public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile document)
    {
        // Validate and process file upload if present
        if (document != null && document.Length > 0)
        {
            var fileExtension = Path.GetExtension(document.FileName).ToLower();
            if (!_allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("document", "Invalid file type. Only PDF, DOCX, and XLSX files are allowed.");
                return View(claim);
            }

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath); // Create directory if not exists
            }

            var filePath = Path.Combine(uploadsPath, document.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await document.CopyToAsync(stream); // Save file to the server
            }

            claim.DocumentPath = $"/uploads/{document.FileName}"; // Store file path in the claim object
        }
        else
        {
            ModelState.AddModelError("document", "Please upload a supporting document.");
            return View(claim);
        }

        _context.Claims.Add(claim); // Add claim to the database
        await _context.SaveChangesAsync(); // Save changes to the database

        return RedirectToAction("ClaimSubmitted"); // Redirect to the confirmation page
    }

    // GET method for the Claim Submitted confirmation page
    [Authorize(Roles = "Lecturer")]
    public IActionResult ClaimSubmitted()
    {
        return View();
    }

    // GET method for viewing pending claims (visible to coordinators and managers)
    [Authorize(Roles = "Coordinator,Manager")]
    [HttpGet]
    public async Task<IActionResult> ViewPendingClaims()
    {
        try
        {
            var pendingClaims = await _context.Claims.Where(c => c.Status == "Pending").ToListAsync(); // Fetch pending claims
            return View(pendingClaims);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while fetching the claims. Please try again later.");
            Console.WriteLine(ex.Message); // Log the error
            return View("Error");
        }
    }

    // POST method for approving a claim
    [Authorize(Roles = "Coordinator,Manager")]
    [HttpPost]
    public async Task<IActionResult> ApproveClaim(int id)
    {
        try
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Approved"; // Update status to approved
                await _context.SaveChangesAsync();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Claim not found.");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while approving the claim. Please try again.");
            Console.WriteLine(ex.Message);
        }
        return RedirectToAction("ViewPendingClaims");
    }

    // POST method for rejecting a claim
    [Authorize(Roles = "Coordinator,Manager")]
    [HttpPost]
    public async Task<IActionResult> RejectClaim(int id)
    {
        try
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = "Rejected"; // Update status to rejected
                await _context.SaveChangesAsync();//await action
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Claim not found.");
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while rejecting the claim. Please try again.");
            Console.WriteLine(ex.Message);
        }
        return RedirectToAction("ViewPendingClaims");
    }

    // GET method for tracking claims (visible to all roles)
    [Authorize(Roles = "Coordinator,Manager,Lecturer")]

    [HttpGet]
    public async Task<IActionResult> TrackClaims()
    {
        try
        {
            var allClaims = await _context.Claims.ToListAsync(); // Fetch all claims
            return View(allClaims);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while fetching the claims. Please try again later.");
            Console.WriteLine(ex.Message);
            return View("Error");
        }
    }

    // POST method for deleting a claim (visible to coordinators and managers)
    [Authorize(Roles = "Coordinator,Manager")]
    [HttpPost]
    public async Task<IActionResult> DeleteClaim(int id)
    {
        var claim = await _context.Claims.FindAsync(id);
        if (claim != null)
        {
            _context.Claims.Remove(claim); // Remove claim from database
            await _context.SaveChangesAsync();
            return RedirectToAction("TrackClaims");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Claim not found.");
            return View("Error");
        }
    }
    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]

}
