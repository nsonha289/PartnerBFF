using FluentAssertions;
using PartnerBFF.Application.Exceptions;
using PartnerBFF.Application.Models.Requests;
using PartnerBFF.Application.Services;
using System.ComponentModel.DataAnnotations;

namespace PartnerBFF.Application.Test.Services
{
    public class TransactionRequestValidationServiceTests
    {
        private readonly TransactionRequestValidationService _validator = new();

        private TransactionRequest ValidRequest() => new()
        {
            PartnerId = "P-1001",
            TransactionReference = "TXN-99823",
            Amount = 250.00m,
            Currency = "USD",
            Timestamp = DateTime.UtcNow.AddMinutes(-1)
        };

        [Fact]
        public void Validate_ShouldPass_WhenRequestIsValid()
        {
            // should not throw
            var act = () => _validator.Validate(ValidRequest());
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_ShouldThrow_WhenAmountIsZero()
        {
            var request = ValidRequest();
            request.Amount = 0;

            var act = () => _validator.Validate(request);
            act.Should().Throw<TransactionValidationException>()
                .Which.Errors.Should().Contain("Amount must be greater than 0.");
        }

        [Fact]
        public void Validate_ShouldThrow_WhenAmountIsNegative()
        {
            var request = ValidRequest();
            request.Amount = -50m;

            var act = () => _validator.Validate(request);
            act.Should().Throw<TransactionValidationException>()
                .Which.Errors.Should().Contain("Amount must be greater than 0.");
        }

        [Fact]
        public void Validate_ShouldThrow_WhenCurrencyIsInvalid()
        {
            var request = ValidRequest();
            request.Currency = "INVALID";

            var act = () => _validator.Validate(request);
            act.Should().Throw<TransactionValidationException>()
                .Which.Errors.Should().Contain("Currency must be a valid ISO 4217 code.");
        }

        [Fact]
        public void Validate_ShouldThrow_WhenPartnerIdIsEmpty()
        {
            var request = ValidRequest();
            request.PartnerId = "";

            var act = () => _validator.Validate(request);
            act.Should().Throw<TransactionValidationException>()
                .Which.Errors.Should().Contain("PartnerId is required.");
        }

        [Fact]
        public void Validate_ShouldThrow_WhenTimestampIsInFuture()
        {
            var request = ValidRequest();
            request.Timestamp = DateTime.UtcNow.AddHours(1);

            var act = () => _validator.Validate(request);
            act.Should().Throw<TransactionValidationException>()
                .Which.Errors.Should().Contain("Timestamp cannot be in the future.");
        }

        [Fact]
        public void Validate_ShouldCollectAllErrors_WhenMultipleFieldsInvalid()
        {
            var request = new TransactionRequest
            {
                PartnerId = "",
                TransactionReference = "",
                Amount = -1,
                Currency = "INVALID",
                Timestamp = DateTime.UtcNow.AddHours(1)
            };

            var act = () => _validator.Validate(request);
            act.Should().Throw<TransactionValidationException>()
                .Which.Errors.Should().HaveCount(5); // all errors collected at once
        }
    }
}
