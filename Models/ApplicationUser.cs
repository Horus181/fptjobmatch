using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class ApplicationUser : IdentityUser
    {

        public bool IsApproved { get; set; } = false;

       

    }
}
