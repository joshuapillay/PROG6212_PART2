using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class ClaimsController : Controller
{
    private readonly ClaimsDbContext _context;

    public ClaimsController(ClaimsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult SubmitClaim()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile document)
    {
        if (document != null && document.Length > 0)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var filePath = Path.Combine(uploadsPath, document.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                document.CopyTo(stream);
            }

            claim.DocumentPath = Path.Combine("/uploads", document.FileName);
        }

        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();

        return RedirectToAction("ClaimSubmitted");
    }

    public IActionResult ClaimSubmitted()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ViewPendingClaims()
    {
        var pendingClaims = await _context.Claims.Where(c => c.Status == "Pending").ToListAsync();
        return View(pendingClaims);
    }

    [HttpPost]
    public async Task<IActionResult> ApproveClaim(int id)
    {
        var claim = await _context.Claims.FindAsync(id);
        if (claim != null)
        {
            claim.Status = "Approved";
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("ViewPendingClaims");
    }

    [HttpPost]
    public async Task<IActionResult> RejectClaim(int id)
    {
        var claim = await _context.Claims.FindAsync(id);
        if (claim != null)
        {
            claim.Status = "Rejected";
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("ViewPendingClaims");
    }

    [HttpGet]
    public async Task<IActionResult> TrackClaims()
    {
        var allClaims = await _context.Claims.ToListAsync();
        return View(allClaims);
    }
}
