namespace PartnerBFF.Application.Exceptions
{
    public class BaseException : Exception
    {
        public int StatusCode { get; }

        protected BaseException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
