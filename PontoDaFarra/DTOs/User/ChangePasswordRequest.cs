using System.ComponentModel.DataAnnotations;

namespace PontoDaFarra.DTOs.User
{
    public class ChangePasswordRequest
    {
        [Required]
        public required string NewPassword { get; set; }
    }
}
