using FluentAssertions;
using PartnerBFF.Application.Models.Requests;
using System.ComponentModel.DataAnnotations;

namespace PartnerBFF.Application.Test.Models
{
    public class TransactionRequestTests
    {
        // ─── Helper ───────────────────────────────────────────────────
        private TransactionRequest ValidRequest() => new()
        {
            PartnerId = "P-1001",
            TransactionReference = "TXN-99823",
            Amount = 250.00m,
            Currency = "USD",
            Timestamp = DateTime.UtcNow.AddMinutes(-1)
        };

        private IList<ValidationResult> Validate(TransactionRequest request)
        {
            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(request, context, results, validateAllProperties: true);
            return results;
        }

        // ─── Valid Request ────────────────────────────────────────────

        [Fact]
        public void Validate_ShouldPass_WhenAllFieldsAreValid()
        {
            var results = Validate(ValidRequest());

            results.Should().BeEmpty();
        }

        // ─── PartnerId ────────────────────────────────────────────────

        [Fact]
        public void Validate_ShouldFail_WhenPartnerIdIsNull()
        {
            var request = ValidRequest();
            request.PartnerId = null;

            var results = Validate(request);

            results.Should().Contain(r =>
                r.MemberNames.Contains(nameof(TransactionRequest.PartnerId)));
        }

        [Fact]
        public void Validate_ShouldFail_WhenPartnerIdIsEmpty()
        {
            var request = ValidRequest();
            request.PartnerId = "";

            var results = Validate(request);

            results.Should().Contain(r =>
                r.MemberNames.Contains(nameof(TransactionRequest.PartnerId)));
        }

        // ─── TransactionReference ─────────────────────────────────────

        [Fact]
        public void Validate_ShouldFail_WhenTransactionReferenceIsNull()
        {
            var request = ValidRequest();
            request.TransactionReference = null;

            var results = Validate(request);

            results.Should().Contain(r =>
                r.MemberNames.Contains(nameof(TransactionRequest.TransactionReference)));
        }

        // ─── Amount ───────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999.99)]
        public void Validate_ShouldFail_WhenAmountIsZeroOrNegative(decimal amount)
        {
            var request = ValidRequest();
            request.Amount = amount;

            var results = Validate(request);

            results.Should().Contain(r =>
                r.MemberNames.Contains(nameof(TransactionRequest.Amount)) &&
                r.ErrorMessage == "Amount must be greater than 0.");
        }

        [Theory]
        [InlineData(0.01)]
        [InlineData(1)]
        [InlineData(999999.99)]
        public void Validate_ShouldPass_WhenAmountIsPositive(decimal amount)
        {
            var request = ValidRequest();
            request.Amount = amount;

            var results = Validate(request);

            results.Should().NotContain(r =>
                r.MemberNames.Contains(nameof(TransactionRequest.Amount)));
        }

        // ─── Currency ─────────────────────────────────────────────────

        [Fact]
        public void Validate_ShouldFail_WhenCurrencyIsNull()
        {
            var request = ValidRequest();
            request.Currency = null;

            var results = Validate(request);

            results.Should().Contain(r =>
                r.MemberNames.Contains(nameof(TransactionRequest.Currency)));
        }

        [Theory]
        [InlineData("INVALID")]
        [InlineData("XYZ")]
        [InlineData("123")]
        [InlineData("")]
        public void Validate_ShouldFail_WhenCurrencyIsNotValidIso(string currency)
        {
            var request = ValidRequest();
            request.Currency = currency;

            var results = Validate(request);

            results.Should().Contain(r =>
                r.MemberNames.Contains(nameof(TransactionRequest.Currency)));
        }

        [Theory]
        [InlineData("USD")]
        [InlineData("EUR")]
        [InlineData("VND")]
        public void Validate_ShouldPass_WhenCurrencyIsValidIso(string currency)
        {
            var request = ValidRequest();
            request.Currency = currency;

            var results = Validate(request);

            results.Should().NotContain(r =>
                r.MemberNames.Contains(nameof(TransactionRequest.Currency)));
        }

        // ─── Timestamp ────────────────────────────────────────────────

        [Fact]
        public void Validate_ShouldFail_WhenTimestampIsDefault()
        {
            var request = ValidRequest();
            request.Timestamp = default;

            var results = Validate(request);

            results.Should().Contain(r =>
                r.MemberNames.Contains(nameof(TransactionRequest.Timestamp)));
        }

        // ─── Multiple Errors ──────────────────────────────────────────

        [Fact]
        public void Validate_ShouldReturnMultipleErrors_WhenMultipleFieldsInvalid()
        {
            var request = new TransactionRequest
            {
                PartnerId = null,
                TransactionReference = null,
                Amount = -1,
                Currency = "INVALID",
                Timestamp = default
            };

            var results = Validate(request);

            results.Should().HaveCountGreaterThan(1);
        }
    }
}
