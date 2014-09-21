using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;

namespace SdojWeb.Models
{
    public class Role : IRole<int>
    {
        public Role()
        {
// ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Users = new HashSet<User>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "½ÇÉ«")]
        public string Name { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}