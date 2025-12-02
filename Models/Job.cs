using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(150)]
        public string EmployerName { get; set; } = string.Empty;

        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Qualifications { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        // Navigation: 1 Job – N JobApplications (nếu bạn dùng)
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();

        public bool IsApproved { get; set; } = false;

    }
}
