using System;

namespace PontoDaFarra.DTOs.User
{
    public class CreateResponse
    {
        public Guid UserId { get; set; }
        public required String Email { get; set; }
        public required String Role { get; set; }
        public required string Name { get; set; }
        public required string RefreshToken { get; set; }
        public required string AccessToken { get; set; }
    }
}
