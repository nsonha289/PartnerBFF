namespace PartnerBFF.Application.Models
{
    public class ErrorResponse
    {
        public string TraceId { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
