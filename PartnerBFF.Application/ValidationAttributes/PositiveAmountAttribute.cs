using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Application.ValidationAttributes
{
    public class PositiveAmountAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object? value,
            ValidationContext validationContext)
        {
            if (value is decimal amount && amount > 0)
                return ValidationResult.Success!;

            return new ValidationResult(
                "Amount must be greater than 0.",
                new[] { validationContext.MemberName! });
        }
    }
}
