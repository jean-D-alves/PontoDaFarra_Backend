using System;

namespace PontoDaFarra.DTOs.Legal
{
    public class UserConsentResponseDto
    {
        public Guid UserConsentId { get; set; }
        public Guid LegalDocumentId { get; set; }
        public required string Type { get; set; }
        public required string Version { get; set; }
        public DateTime AcceptedAt { get; set; }
        public string? IpAddress { get; set; }
    }
}
