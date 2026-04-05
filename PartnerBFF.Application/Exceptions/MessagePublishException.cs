using Microsoft.AspNetCore.Http;

namespace PartnerBFF.Application.Exceptions
{
    public class MessagePublishException : BaseException
    {
        public MessagePublishException(string message)
        : base(message, StatusCodes.Status503ServiceUnavailable) { }
    }
}
