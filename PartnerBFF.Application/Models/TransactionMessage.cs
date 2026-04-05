using PartnerBFF.Application.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Application.Models
{
    public class TransactionMessage
    {
        public string PartnerId { get; private set; }
        public string TransactionReference { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public DateTime Timestamp { get; private set; }
        public DateTime QueuedAt { get; private set; }

        public TransactionMessage(TransactionRequest request)
        {
            PartnerId = request.PartnerId;
            TransactionReference = request.TransactionReference;
            Amount = request.Amount;
            Currency = request.Currency;
            Timestamp = request.Timestamp;
            QueuedAt = DateTime.UtcNow;
        }
    }
}
