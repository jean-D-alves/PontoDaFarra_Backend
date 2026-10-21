using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PontoDaFarra.models
{
    public class User
    {
        public Guid UserId { get; set; }
        [StringLength(80)]
        public required String Name { get; set; }
        [EmailAddress]
        public required String Email { get; set; }
        [StringLength(80)]
        public required String Password { get; set; }
        [Phone]
        public String? PhoneNumber { get; set; }
        public string Role { get; set; } = "Client";
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        
    }
}