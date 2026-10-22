using System.ComponentModel.DataAnnotations;

namespace PontoDaFarra.DTOs.User
{
    public class UserCreate
    {
        public required String Name { get; set; }
        [Required, EmailAddress]
        public required String Email { get; set; }
        [Required]
        public required String Password { get; set; }
        public required String PhoneNumber { get; set; }
    }
}
