using FluentAssertions;
using PartnerBFF.Application.Models.Requests;
using PartnerBFF.Application.ValidationAttributes;
using System.ComponentModel.DataAnnotations;


namespace PartnerBFF.Application.Test.ValidationAttributes
{
    public class AllowedCurrencyAttributeTests
    {
        private readonly AllowedCurrencyAttribute _attribute = new();

        private ValidationResult? Validate(object? value) =>
            _attribute.GetValidationResult(
                value,
                new ValidationContext(new TransactionRequest())
                {
                    MemberName = nameof(TransactionRequest.Currency)
                });

        // ─── Valid Currencies ─────────────────────────────────────────

        [Theory]
        [InlineData("USD")]
        [InlineData("EUR")]
        [InlineData("VND")]
        public void IsValid_ShouldReturnSuccess_WhenCurrencyIsAllowed(string currency)
        {
            var result = Validate(currency);

            result.Should().Be(ValidationResult.Success);
        }

        // ─── Invalid Currencies ───────────────────────────────────────

        [Theory]
        [InlineData("INVALID")]
        [InlineData("XYZ")]
        [InlineData("123")]
        [InlineData("usd")]   // ← case sensitive
        [InlineData("Us")]
        public void IsValid_ShouldFail_WhenCurrencyIsNotInAllowedList(string currency)
        {
            var result = Validate(currency);

            result.Should().NotBe(ValidationResult.Success);
            result!.MemberNames.Should().Contain(nameof(TransactionRequest.Currency));
        }

        [Fact]
        public void IsValid_ShouldFail_WhenCurrencyIsEmpty()
        {
            var result = Validate("");

            result.Should().NotBe(ValidationResult.Success);
            result!.MemberNames.Should().Contain(nameof(TransactionRequest.Currency));
        }

        [Fact]
        public void IsValid_ShouldFail_WhenCurrencyIsNull()
        {
            var result = Validate(null);

            result.Should().NotBe(ValidationResult.Success);
            result!.MemberNames.Should().Contain(nameof(TransactionRequest.Currency));
        }

        // ─── Error Message ────────────────────────────────────────────

        [Fact]
        public void IsValid_ShouldReturnErrorMessage_WhenCurrencyIsInvalid()
        {
            var result = Validate("INVALID");

            result!.ErrorMessage.Should().NotBeNullOrEmpty();
        }
    }
}
