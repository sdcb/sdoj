using SdojWeb.Models;

namespace SdojWeb.Infrastructure.Identity
{
    public interface ICurrentUser
    {
        UsefulUserModel User { get; }
    }
}
