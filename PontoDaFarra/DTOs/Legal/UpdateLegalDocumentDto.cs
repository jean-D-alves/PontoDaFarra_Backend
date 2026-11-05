using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PontoDaFarra.DTOs.Legal
{
    public class UpdateLegalDocumentDto : IValidatableObject
    {
        [MaxLength(20)]
        public string? Version { get; set; }

        public string? Content { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Version is null && Content is null)
            {
                yield return new ValidationResult(
                    "At least one field must be provided",
                    [nameof(Version), nameof(Content)]);
            }
        }
    }
}
