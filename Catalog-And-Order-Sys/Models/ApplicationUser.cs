using Microsoft.AspNetCore.Identity;

namespace Catalog_And_Order_Sys.Models
{
    // Kế thừa IdentityUser giống lab21, không cần RefreshToken vì
    // dùng Cookie Authentication (MVC) thay vì JWT (API)
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
