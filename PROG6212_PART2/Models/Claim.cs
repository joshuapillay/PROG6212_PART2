using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Claim
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string LecturerName { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public double HoursWorked { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public double HourlyRate { get; set; }

    public string Notes { get; set; }

    [Required]
    public string Status { get; set; } = "Pending"; // Default status is pending

    [Required]
    public string DocumentPath { get; set; } // Path to the uploaded file
}
