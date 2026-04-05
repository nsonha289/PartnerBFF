using PartnerBFF.Application.Constants;
using PartnerBFF.Application.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace PartnerBFF.Application.Models.Requests
{
    public class TransactionRequest
    {
        [Required]
        public string PartnerId { get; set; }
        [Required]
        public string TransactionReference { get; set; }
        [Required]
        [PositiveAmount]
        public decimal Amount { get; set; }
        [Required]
        [AllowedCurrency]
        public string Currency { get; set; }
        [Required]
        [AllowedTimestamp]
        public DateTime Timestamp { get; set; }
    }
}
