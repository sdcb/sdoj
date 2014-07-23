using SdojWeb.Infrastructure.Mapping;

namespace SdojWeb.Models
{
    public class UsefulUserModel : IMapFrom<User>
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }
    }
}