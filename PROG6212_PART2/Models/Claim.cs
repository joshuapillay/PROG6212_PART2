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

    [NotMapped]
    public double TotalPayment => HoursWorked * HourlyRate;

    public string Notes { get; set; }

    [Required]
    public string Status { get; set; } = "Pending";

    public string? RejectionReason { get; set; } 

    [Required]
    public string? DocumentPath { get; set; }
}
