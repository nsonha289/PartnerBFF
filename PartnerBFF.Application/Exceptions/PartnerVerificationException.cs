using Microsoft.AspNetCore.Http;

namespace PartnerBFF.Application.Exceptions
{
    public class PartnerVerificationException : BaseException
    {
        public PartnerVerificationException(string message)
        : base(message, StatusCodes.Status422UnprocessableEntity) { }
    }
}
