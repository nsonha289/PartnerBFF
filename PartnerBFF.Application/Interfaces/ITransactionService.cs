using PartnerBFF.Application.Models.Requests;

namespace PartnerBFF.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionStatusEnum> ProcessTransaction(TransactionRequest request, CancellationToken cancellationToken);
    }
}
