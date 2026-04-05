using PartnerBFF.Application.Exceptions;
using PartnerBFF.Application.Interfaces;
using PartnerBFF.Application.Models.Requests;

namespace PartnerBFF.Application.Services
{
    public class TransactionRequestValidationService : IRequestValidationService<TransactionRequest>
    {
        public void Validate(TransactionRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.PartnerId))
                errors.Add("PartnerId is required.");

            if (string.IsNullOrWhiteSpace(request.TransactionReference))
                errors.Add("TransactionReference is required.");

            if (request.Amount <= 0)
                errors.Add("Amount must be greater than 0.");

            if (string.IsNullOrWhiteSpace(request.Currency))
                errors.Add("Currency is required.");
            else if (!AppConstant.ValidCurrencies.Contains(request.Currency))
                errors.Add("Currency must be a valid ISO 4217 code.");

            if (request.Timestamp == default)
                errors.Add("Timestamp is required.");
            else if (request.Timestamp > DateTime.UtcNow)
                errors.Add("Timestamp cannot be in the future.");

            if (errors.Any())
                throw new TransactionValidationException(errors);
        }
    }
}
