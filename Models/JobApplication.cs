using System.ComponentModel.DataAnnotations;
using WebApplication2.Models;

public class JobApplication
{
    public int Id { get; set; }

    [Required]
    public int JobId { get; set; }
    public Job? Job { get; set; }

    [Required]
    public string JobSeekerId { get; set; } = string.Empty;
    public ApplicationUser? JobSeeker { get; set; }

    [Required]
    [StringLength(150)]
    public string ApplicantName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ApplicantEmail { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Introduction { get; set; } = string.Empty;

    // ĐỔI TÊN PROPERTY Ở ĐÂY
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public string? CvFilePath { get; set; }
}
