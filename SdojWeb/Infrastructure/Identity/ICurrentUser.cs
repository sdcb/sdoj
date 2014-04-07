using SdojWeb.Models;

namespace SdojWeb.Infrastructure.Identity
{
    public interface ICurrentUser
    {
        ApplicationUser User { get; }
    }
}
