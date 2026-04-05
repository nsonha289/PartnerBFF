using Microsoft.Extensions.Logging;
using PartnerBFF.Application.Exceptions;
using PartnerBFF.Application.Interfaces;
using PartnerBFF.Application.Models;
using PartnerBFF.Application.Models.Requests;

namespace PartnerBFF.Application.Services
{
    public class TransactionService: ITransactionService
    {
        private readonly IMessagePublisherBroker _messagePublisherBroker;
        private readonly IPartnerVerifierService _partnerVerifierService;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            IMessagePublisherBroker messagePublisherBroker, 
            IPartnerVerifierService partnerVerifierService,
            ILogger<TransactionService> logger)
        {
            _messagePublisherBroker = messagePublisherBroker;
            _partnerVerifierService = partnerVerifierService;
            _logger = logger;
        }

        public async Task<TransactionStatusEnum> ProcessTransaction(TransactionRequest request, CancellationToken cancellationToken)
        {
            if (!await _partnerVerifierService.VerifyPartnerAsync(request.PartnerId))
            {
                _logger.LogWarning("Invalid partner ID: {PartnerId}", request.PartnerId);
                throw new PartnerVerificationException("Invalid partner ID.");
            }

            var status = TransactionStatusEnum.Initialized;
            try
            {
                await _messagePublisherBroker.PublishAsync(new TransactionMessage(request), cancellationToken);
                status = TransactionStatusEnum.Published;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish transaction request for reference: {TransactionReference}", request.TransactionReference);
                status = TransactionStatusEnum.Failed;
            }
            return status;
        }
    }
}
