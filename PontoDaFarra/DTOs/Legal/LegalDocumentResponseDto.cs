using System;

namespace PontoDaFarra.DTOs.Legal
{
    public class LegalDocumentResponseDto
    {
        public Guid LegalDocumentId { get; set; }
        public required string Type { get; set; }
        public required string Version { get; set; }
        public required string Content { get; set; }
        public DateTime PublishedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
