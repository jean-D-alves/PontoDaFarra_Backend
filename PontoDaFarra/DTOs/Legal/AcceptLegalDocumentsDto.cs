using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PontoDaFarra.DTOs.Legal
{
    public class AcceptLegalDocumentsDto
    {
        [Required]
        [MinLength(1)]
        public List<Guid> LegalDocumentIds { get; set; } = [];
    }
}
