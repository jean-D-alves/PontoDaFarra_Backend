using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PontoDaFarra.models
{
    public enum LegalDocumentType { PrivacyPolicy = 1, TermsOfService = 2 }

    public class LegalDocument
    {
        public Guid LegalDocumentId { get; set; }
        public LegalDocumentType Type { get; set; }
        [MaxLength(20)]
        public required string Version { get; set; }
        public required string Content { get; set; }
        public DateTime PublishedAt { get; set; }
        public bool IsActive { get; set; }
        public ICollection<UserConsent> Consents { get; set; } = new List<UserConsent>();
    }
}
