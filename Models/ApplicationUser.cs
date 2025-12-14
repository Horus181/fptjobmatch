using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace WebApplication2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<JobApplication> JobApplications { get; set; }
            = new List<JobApplication>();
    }
}
