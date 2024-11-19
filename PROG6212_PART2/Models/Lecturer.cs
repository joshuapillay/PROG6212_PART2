using System.ComponentModel.DataAnnotations;

public class Lecturer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Phone]
    public string ContactNumber { get; set; }
}

