using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; }
        public Job? Job { get; set; }

        [Required]
        [StringLength(150)]
        public string ApplicantName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string ApplicantEmail { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Introduction { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.Now;
    }
}
