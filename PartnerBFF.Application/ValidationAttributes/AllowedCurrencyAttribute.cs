using PartnerBFF.Application.Constants;
using System.ComponentModel.DataAnnotations;

namespace PartnerBFF.Application.ValidationAttributes
{
    public class AllowedCurrencyAttribute : ValidationAttribute
    {
        private readonly string[] _allowedCurrencies;

        public AllowedCurrencyAttribute()
        {
            _allowedCurrencies = CurrencyCodes.Allowed;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string currency && _allowedCurrencies.Contains(currency))
                return ValidationResult.Success!;

            return new ValidationResult(
                $"Currency must be one of: {string.Join(", ", _allowedCurrencies)}",
                new[] { validationContext.MemberName! });
        }
    }
}
