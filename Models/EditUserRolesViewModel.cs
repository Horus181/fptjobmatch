namespace WebApplication2.Models
{
    public class EditUserRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<RoleOption> Roles { get; set; } = new();

        public class RoleOption
        {
            public string RoleName { get; set; } = string.Empty;
            public bool IsSelected { get; set; }
        }
    }
}
