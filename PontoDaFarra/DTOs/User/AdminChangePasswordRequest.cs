using System.ComponentModel.DataAnnotations;

namespace PontoDaFarra.DTOs.User
{
    public class AdminChangePasswordRequest
    {
        [Required]
        public required string NewPassword { get; set; }
    }
}
