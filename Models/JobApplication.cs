using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class JobApplication
    {
   
        public int Id { get; set; }

        public int JobId { get; set; }
        public Job? Job { get; set; }

        public string JobSeekerId { get; set; } = string.Empty;
        public ApplicationUser? JobSeeker { get; set; }

        [Required]
        [StringLength(150)]
        public string ApplicantName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string ApplicantEmail { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Introduction { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public bool IsViewedByEmployer { get; set; } = false;
    }
}
