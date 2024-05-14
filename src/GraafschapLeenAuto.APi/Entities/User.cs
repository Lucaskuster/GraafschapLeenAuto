using System.ComponentModel.DataAnnotations;

namespace GraafschapLeenAuto.Api.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public virtual List<Role> Roles { get; set; } = new List<Role>();
    }
}
