using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

public class ClaimsController : Controller
{
    // Static list to store claims in memory
    private static List<Claim> claims = new List<Claim>();

    // GET: Lecturer form for submitting claims
    [HttpGet]
    public IActionResult SubmitClaim()
    {
        return View();
    }

    // POST: Submit claim (with file upload)
    [HttpPost]
    public IActionResult SubmitClaim(Claim claim, IFormFile document)
    {
        if (document != null && document.Length > 0)
        {
            // Define the uploads path inside wwwroot
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            // Ensure the directory exists
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Combine file name with the uploads path
            var filePath = Path.Combine(uploadsPath, document.FileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                document.CopyTo(stream);
            }

            // Save the relative file path (starting from wwwroot)
            claim.DocumentPath = Path.Combine("/uploads", document.FileName);
        }

        // Assign a new ID to the claim
        claim.Id = claims.Count + 1;

        // Add the claim to the in-memory list
        claims.Add(claim);

        // Redirect to a confirmation page
        return RedirectToAction("ClaimSubmitted");
    }


    // A confirmation page after submission
    public IActionResult ClaimSubmitted()
    {
        return View();
    }

    // GET: Display all pending claims for Coordinators/Managers
    [HttpGet]
    public IActionResult ViewPendingClaims()
    {
        return View(claims.Where(c => c.Status == "Pending").ToList());
    }

    // POST: Approve a claim
    [HttpPost]
    public IActionResult ApproveClaim(int id)
    {
        var claim = claims.FirstOrDefault(c => c.Id == id);
        if (claim != null)
        {
            claim.Status = "Approved";
        }
        return RedirectToAction("ViewPendingClaims");
    }

    // POST: Reject a claim
    [HttpPost]
    public IActionResult RejectClaim(int id)
    {
        var claim = claims.FirstOrDefault(c => c.Id == id);
        if (claim != null)
        {
            claim.Status = "Rejected";
        }
        return RedirectToAction("ViewPendingClaims");
    }

    // GET: View all claims to track their progress
    [HttpGet]
    public IActionResult TrackClaims()
    {
        return View(claims);
    }
}
