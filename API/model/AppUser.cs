using Microsoft.AspNetCore.Identity;

namespace API.model
{
    public class AppUser : IdentityUser<Guid>
    {
        public virtual ICollection<FileEntity> Files { get; set; } = [];
    }
}