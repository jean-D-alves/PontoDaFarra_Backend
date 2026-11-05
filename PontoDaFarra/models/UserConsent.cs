using System;
using System.ComponentModel.DataAnnotations;

namespace PontoDaFarra.models
{
    public class UserConsent
    {
        public Guid UserConsentId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public Guid LegalDocumentId { get; set; }
        public LegalDocument LegalDocument { get; set; } = default!;
        public DateTime AcceptedAt { get; set; }
        [MaxLength(45)]
        public string? IpAddress { get; set; }
    }
}
