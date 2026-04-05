using System.ComponentModel.DataAnnotations;

namespace PartnerBFF.Application.Models.Requests
{
    public class TransactionRequest
    {
        public string PartnerId { get; set; }
        public string TransactionReference { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
