using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Application.Models.Responses
{
    public class TransactionResponse
    {
        public string TransactionReference { get; set; }
        public string Status { get; set; }  // e.g. "Queued"
        public DateTime ReceivedAt { get; set; }

        public TransactionResponse(string transactionReference, string status)
        {
            TransactionReference = transactionReference;
            Status = status;
            ReceivedAt = DateTime.UtcNow;
        }
    }
}
