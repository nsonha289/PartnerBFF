using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PartnerBFF.Application.Exceptions;
using PartnerBFF.Application.Interfaces;
using PartnerBFF.Application.Models;
using PartnerBFF.Application.Models.Requests;
using PartnerBFF.Application.Services;

namespace PartnerBFF.Application.Test.Services
{
    public class TransactionServiceTests
    {
        // ─── Mocks ────────────────────────────────────────────────────
        private readonly IMessagePublisherBroker _messagePublisherBroker
            = Substitute.For<IMessagePublisherBroker>();

        private readonly IPartnerVerifierService _partnerVerifierService
            = Substitute.For<IPartnerVerifierService>();

        private readonly ILogger<TransactionService> _logger
            = Substitute.For<ILogger<TransactionService>>();

        // ─── Setup ────────────────────────────────────────────────────
        private TransactionService CreateService() =>
            new(_messagePublisherBroker,
                _partnerVerifierService,
                _logger);

        private TransactionRequest ValidRequest() => new()
        {
            PartnerId = "P-1001",
            TransactionReference = "TXN-99823",
            Amount = 250.00m,
            Currency = "USD",
            Timestamp = DateTime.UtcNow.AddMinutes(-1)
        };

        // ─── Partner Verification Tests ───────────────────────────────

        [Fact]
        public async Task ProcessTransaction_ShouldThrow_WhenPartnerNotVerified()
        {
            _partnerVerifierService
                .VerifyPartnerAsync(Arg.Any<string>())
                .Returns(false);

            var act = async () =>
                await CreateService()
                    .ProcessTransaction(ValidRequest(), CancellationToken.None);

            await act.Should().ThrowAsync<PartnerVerificationException>()
                .WithMessage("Invalid partner ID.");

            await _messagePublisherBroker
                .DidNotReceive()
                .PublishAsync(Arg.Any<TransactionMessage>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ProcessTransaction_ShouldCallVerifier_WithCorrectPartnerId()
        {
            var request = ValidRequest();

            _partnerVerifierService
                .VerifyPartnerAsync(Arg.Any<string>())
                .Returns(true);

            await CreateService().ProcessTransaction(request, CancellationToken.None);

            await _partnerVerifierService
                .Received(1)
                .VerifyPartnerAsync(request.PartnerId);
        }

        // ─── Publish Tests ────────────────────────────────────────────

        [Fact]
        public async Task ProcessTransaction_ShouldReturnPublished_WhenSuccessful()
        {
            _partnerVerifierService
                .VerifyPartnerAsync(Arg.Any<string>())
                .Returns(true);

            var result = await CreateService()
                .ProcessTransaction(ValidRequest(), CancellationToken.None);

            result.Should().Be(TransactionStatusEnum.Published);
        }

        [Fact]
        public async Task ProcessTransaction_ShouldReturnFailed_WhenPublishThrows()
        {
            _partnerVerifierService
                .VerifyPartnerAsync(Arg.Any<string>())
                .Returns(true);

            _messagePublisherBroker
                .PublishAsync(Arg.Any<TransactionMessage>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("RabbitMQ unavailable"));

            var result = await CreateService()
                .ProcessTransaction(ValidRequest(), CancellationToken.None);

            result.Should().Be(TransactionStatusEnum.Failed);
        }

        [Fact]
        public async Task ProcessTransaction_ShouldNeverReturnInitialized_AfterProcessing()
        {
            _partnerVerifierService
                .VerifyPartnerAsync(Arg.Any<string>())
                .Returns(true);

            var result = await CreateService()
                .ProcessTransaction(ValidRequest(), CancellationToken.None);

            result.Should().NotBe(TransactionStatusEnum.Initialized);
        }

        [Theory]
        [InlineData(true, TransactionStatusEnum.Published)]
        [InlineData(false, TransactionStatusEnum.Failed)]
        public async Task ProcessTransaction_ShouldReturnCorrectStatus_BasedOnPublishResult(
            bool publishSucceeds,
            TransactionStatusEnum expectedStatus)
        {
            _partnerVerifierService
                .VerifyPartnerAsync(Arg.Any<string>())
                .Returns(true);

            if (publishSucceeds)
                _messagePublisherBroker
                    .PublishAsync(Arg.Any<TransactionMessage>(), Arg.Any<CancellationToken>())
                    .Returns(Task.CompletedTask);
            else
                _messagePublisherBroker
                    .PublishAsync(Arg.Any<TransactionMessage>(), Arg.Any<CancellationToken>())
                    .Throws(new Exception("Publish failed"));

            var result = await CreateService()
                .ProcessTransaction(ValidRequest(), CancellationToken.None);

            result.Should().Be(expectedStatus);
        }
    }
}
