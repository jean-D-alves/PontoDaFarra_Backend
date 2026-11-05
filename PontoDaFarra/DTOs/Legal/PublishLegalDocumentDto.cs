using System.ComponentModel.DataAnnotations;
using PontoDaFarra.models;

namespace PontoDaFarra.DTOs.Legal
{
    public class PublishLegalDocumentDto
    {
        [Required]
        [EnumDataType(typeof(LegalDocumentType))]
        public LegalDocumentType Type { get; set; }

        [Required]
        [MaxLength(20)]
        public required string Version { get; set; }

        [Required]
        public required string Content { get; set; }
    }
}
