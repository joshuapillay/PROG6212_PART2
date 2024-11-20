using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using OfficeOpenXml;

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
        if (claim.HoursWorked <= 0 || claim.HourlyRate <= 0)
        {
            ModelState.AddModelError(string.Empty, "Hours Worked and Hourly Rate must be positive values.");
            return View(claim);
        }

        // Auto-calculate payment and set to pending
        claim.Status = "Pending";

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
                Directory.CreateDirectory(uploadsPath);
            }

            var filePath = Path.Combine(uploadsPath, document.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await document.CopyToAsync(stream);
            }

            claim.DocumentPath = $"/uploads/{document.FileName}";
        }
        else
        {
            ModelState.AddModelError("document", "Please upload a supporting document.");
            return View(claim);
        }

        // Automated claim verification
        if (claim.HoursWorked > 252) // Example: limit of 40 hours
        {
            claim.Status = "Rejected";
            claim.Notes = "Rejected: Hours worked exceed the maximum allowed limit of 252.";
        }
        else if (claim.HourlyRate > 200) // Example: hourly rate limit
        {
            claim.Status = "Rejected";
            claim.Notes = "Rejected: Hourly rate exceeds the maximum allowed limit of 200.";
        }
        else
        {
            claim.Status = "Pending";
        }

        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();

        return RedirectToAction("ClaimSubmitted");
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
            var pendingClaims = await _context.Claims
                .Where(c => c.Status == "Pending")
                .ToListAsync();
            return View(pendingClaims);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while fetching the claims. Please try again later.");
            Console.WriteLine(ex.Message);
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
    // roles defined below
    // GET method for tracking claims (visible to all roles)
    [Authorize(Roles = "Coordinator,Manager,Lecturer,HR")]

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
    // roles defined below
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

    [Authorize(Roles = "HR")]
    [HttpGet]
    public async Task<IActionResult> HRView()
    {
        var approvedClaims = await _context.Claims
            .Where(c => c.Status == "Approved")
            .ToListAsync();

        return View(approvedClaims);
    }

    [Authorize(Roles = "HR")]
    [HttpGet]
    public async Task<IActionResult> GenerateReport()
    {
        var approvedClaims = await _context.Claims
            .Where(c => c.Status == "Approved")
            .ToListAsync();

        // Generate a PDF report (for simplicity, using plain text here)
        var reportContent = "Approved Claims Report\n\n";
        reportContent += "LecturerName, HoursWorked, HourlyRate, TotalPayment\n";

        foreach (var claim in approvedClaims)
        {
            var totalPayment = claim.HoursWorked * claim.HourlyRate;
            reportContent += $"{claim.LecturerName}, {claim.HoursWorked}, {claim.HourlyRate}, {totalPayment}\n";
        }

        var fileName = $"ApprovedClaimsReport_{DateTime.Now:yyyyMMdd}.txt";
        var bytes = System.Text.Encoding.UTF8.GetBytes(reportContent);

        return File(bytes, "text/plain", fileName);
    }


    [Authorize(Roles = "HR")]
    [HttpGet]
    public async Task<IActionResult> ManageLecturers()
    {
        var lecturers = await _context.Lecturers.ToListAsync();
        return View(lecturers);
    }

    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> UpdateLecturer(Lecturer lecturer)
    {
        if (!ModelState.IsValid)
        {
            return View("ManageLecturers");
        }

        _context.Lecturers.Update(lecturer);
        await _context.SaveChangesAsync();
        return RedirectToAction("ManageLecturers");
    }


    public IActionResult GeneratePdfReport()
    {
        var claims = _context.Claims.Where(c => c.Status == "Approved").ToList();

        // Create a temporary file path for the PDF
        string filePath = Path.Combine(Path.GetTempPath(), "ApprovedClaimsReport.pdf");

        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            using (var writer = new StreamWriter(fileStream))
            {
                writer.WriteLine("Approved Claims Report");
                writer.WriteLine("Generated on: " + DateTime.Now.ToString("g"));
                writer.WriteLine("-----------------------------------");

                // Add headers
                writer.WriteLine($"{"Lecturer Name",-20} {"Hours Worked",-15} {"Hourly Rate",-15} {"Total Payment",-15}");

                // Add claim details
                foreach (var claim in claims)
                {
                    writer.WriteLine($"{claim.LecturerName,-20} {claim.HoursWorked,-15} {claim.HourlyRate,-15:C} {(claim.HoursWorked * claim.HourlyRate),-15:C}");
                }

                writer.WriteLine("-----------------------------------");
            }
        }

        // Return the PDF file for download
        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, "application/pdf", "ApprovedClaimsReport.pdf");
    }


    public IActionResult GenerateExcelReport()
    {
        // Set the license context (required for EPPlus)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Approved Claims Report");

            // Add headers
            worksheet.Cells[1, 1].Value = "Lecturer Name";
            worksheet.Cells[1, 2].Value = "Hours Worked";
            worksheet.Cells[1, 3].Value = "Hourly Rate";
            worksheet.Cells[1, 4].Value = "Total Payment";

            // Fetch approved claims
            var claims = _context.Claims.Where(c => c.Status == "Approved").ToList();

            // Add data rows
            int row = 2;
            foreach (var claim in claims)
            {
                worksheet.Cells[row, 1].Value = claim.LecturerName;
                worksheet.Cells[row, 2].Value = claim.HoursWorked;
                worksheet.Cells[row, 3].Value = claim.HourlyRate;
                worksheet.Cells[row, 4].Value = claim.HoursWorked * claim.HourlyRate;
                row++;
            }

            // Auto-fit columns for better formatting
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Generate the Excel file in memory
            var excelData = package.GetAsByteArray();

            // Return the file for download
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ApprovedClaimsReport.xlsx");
        }
    }



    //Mrzygłód, K., 2022. Azure for Developers. 2nd ed. August: [Meeta Rajani]

}
