using FluentAssertions;
using PartnerBFF.Application.Models.Requests;
using PartnerBFF.Application.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Application.Test.ValidationAttributes
{
    public class PositiveAmountAttributeTests
    {
        private readonly PositiveAmountAttribute _attribute = new();

        private ValidationResult? Validate(object? value) =>
            _attribute.GetValidationResult(
                value,
                new ValidationContext(new TransactionRequest())
                {
                    MemberName = nameof(TransactionRequest.Amount)
                });

        // ─── Valid Amounts ────────────────────────────────────────────

        [Theory]
        [InlineData(0.01)]
        [InlineData(0.50)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(250.00)]
        [InlineData(999999.99)]
        public void IsValid_ShouldReturnSuccess_WhenAmountIsPositive(decimal amount)
        {
            var result = Validate(amount);

            result.Should().Be(ValidationResult.Success);
        }

        // ─── Invalid Amounts ──────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-0.01)]
        [InlineData(-1)]
        [InlineData(-999.99)]
        public void IsValid_ShouldFail_WhenAmountIsZeroOrNegative(decimal amount)
        {
            var result = Validate(amount);

            result.Should().NotBe(ValidationResult.Success);
            result!.MemberNames.Should().Contain(nameof(TransactionRequest.Amount));
        }

        [Fact]
        public void IsValid_ShouldFail_WhenValueIsNull()
        {
            var result = Validate(null);

            result.Should().NotBe(ValidationResult.Success);
            result!.MemberNames.Should().Contain(nameof(TransactionRequest.Amount));
        }

        // ─── Error Message ────────────────────────────────────────────

        [Fact]
        public void IsValid_ShouldReturnCorrectErrorMessage_WhenAmountIsInvalid()
        {
            var result = Validate(0m);

            result!.ErrorMessage.Should().Be("Amount must be greater than 0.");
        }
    }
}
