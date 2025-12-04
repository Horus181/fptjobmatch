using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace WebApplication2.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Các property bổ sung nếu bạn có...

        // Danh sách application mà user đã nộp (nếu là JobSeeker)
        public ICollection<JobApplication> JobApplications { get; set; }
            = new List<JobApplication>();
    }
}
