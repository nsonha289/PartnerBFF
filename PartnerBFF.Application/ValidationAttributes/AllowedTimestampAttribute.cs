using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Application.ValidationAttributes
{
    public class AllowedTimestampAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(
        object? value,
        ValidationContext validationContext)
        {
            if (value is not DateTime timestamp || timestamp == default)
                return new ValidationResult(
                    "Timestamp is required.",
                    new[] { validationContext.MemberName! });

            if (timestamp > DateTime.UtcNow)
                return new ValidationResult(
                    "Timestamp cannot be in the future.",
                    new[] { validationContext.MemberName! });

            return ValidationResult.Success!;
        }
    }
}
