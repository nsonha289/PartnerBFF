using FluentAssertions;
using PartnerBFF.Application.Models.Requests;
using PartnerBFF.Application.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace PartnerBFF.Application.Test.ValidationAttributes
{
    public class AllowedTimestampAttributeTests
    {
        private readonly AllowedTimestampAttribute _attribute = new();

        private ValidationResult? Validate(object? value) =>
            _attribute.GetValidationResult(
                value,
                new ValidationContext(new TransactionRequest())
                {
                    MemberName = nameof(TransactionRequest.Timestamp)
                });

        // ─── Valid Timestamps ─────────────────────────────────────────

        [Fact]
        public void IsValid_ShouldReturnSuccess_WhenTimestampIsInThePast()
        {
            var result = Validate(DateTime.UtcNow.AddMinutes(-1));

            result.Should().Be(ValidationResult.Success);
        }

        [Fact]
        public void IsValid_ShouldReturnSuccess_WhenTimestampIsNow()
        {
            // small buffer to avoid flakiness
            var result = Validate(DateTime.UtcNow.AddSeconds(-1));

            result.Should().Be(ValidationResult.Success);
        }

        [Fact]
        public void IsValid_ShouldReturnSuccess_WhenTimestampIsYesterday()
        {
            var result = Validate(DateTime.UtcNow.AddDays(-1));

            result.Should().Be(ValidationResult.Success);
        }

        // ─── Invalid Timestamps ───────────────────────────────────────

        [Fact]
        public void IsValid_ShouldFail_WhenTimestampIsDefault()
        {
            var result = Validate(default(DateTime));

            result.Should().NotBe(ValidationResult.Success);
            result!.ErrorMessage.Should().Be("Timestamp is required.");
            result.MemberNames.Should().Contain(nameof(TransactionRequest.Timestamp));
        }

        [Fact]
        public void IsValid_ShouldFail_WhenTimestampIsInFuture()
        {
            var result = Validate(DateTime.UtcNow.AddHours(1));

            result.Should().NotBe(ValidationResult.Success);
            result!.ErrorMessage.Should().Be("Timestamp cannot be in the future.");
            result.MemberNames.Should().Contain(nameof(TransactionRequest.Timestamp));
        }

        [Fact]
        public void IsValid_ShouldFail_WhenTimestampIsTomorrow()
        {
            var result = Validate(DateTime.UtcNow.AddDays(1));

            result.Should().NotBe(ValidationResult.Success);
            result!.ErrorMessage.Should().Be("Timestamp cannot be in the future.");
            result.MemberNames.Should().Contain(nameof(TransactionRequest.Timestamp));
        }

        [Fact]
        public void IsValid_ShouldFail_WhenValueIsNull()
        {
            var result = Validate(null);

            result.Should().NotBe(ValidationResult.Success);
            result!.ErrorMessage.Should().Be("Timestamp is required.");
            result.MemberNames.Should().Contain(nameof(TransactionRequest.Timestamp));
        }

        // ─── Error Message Priority ───────────────────────────────────

        [Fact]
        public void IsValid_ShouldReturnRequiredError_BeforeFutureError_WhenDefault()
        {
            // default(DateTime) is 01/01/0001 which is also in the past
            // but "required" check should take priority over "future" check
            var result = Validate(default(DateTime));

            result!.ErrorMessage.Should().Be("Timestamp is required.");
        }
    }
}
