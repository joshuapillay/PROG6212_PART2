public class Claim
{
    public int Id { get; set; }
    public string LecturerName { get; set; }
    public double HoursWorked { get; set; }
    public double HourlyRate { get; set; }
    public string Notes { get; set; }
    public string Status { get; set; } = "Pending"; // Default status is pending
    public string DocumentPath { get; set; } // Path to the uploaded file
}
