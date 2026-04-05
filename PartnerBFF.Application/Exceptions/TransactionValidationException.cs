using Microsoft.AspNetCore.Http;

namespace PartnerBFF.Application.Exceptions
{
    public class TransactionValidationException : BaseException
    {
        public IEnumerable<string> Errors { get; } = new List<string>();
        public TransactionValidationException(IEnumerable<string> errors)
        : base("Validation failed", StatusCodes.Status400BadRequest) 
        {
            Errors = errors;
        }    
    }
}
