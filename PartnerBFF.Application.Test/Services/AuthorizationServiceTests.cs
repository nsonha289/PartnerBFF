using FluentAssertions;
using NSubstitute.ExceptionExtensions;
using PartnerBFF.Application.Services;
using System.Security.Claims;


namespace PartnerBFF.Application.Test.Services
{
    public class AuthorizationServiceTests
    {
        private readonly AuthorizationService _service = new();

        private ClaimsPrincipal BuildUser(string partnerId) =>
            new(new ClaimsIdentity(new[]
            {
            new Claim("PartnerId", partnerId)
            }));

        [Fact]
        public void IsAuthorized_ShouldReturnTrue_WhenPartnerIdMatches()
        {
            var user = BuildUser("P-1001");

            var result = _service.IsAuthorized(user, "P-1001");

            result.Should().BeTrue();
        }

        [Fact]
        public void IsAuthorized_ShouldReturnFalse_WhenPartnerIdDoesNotMatch()
        {
            var user = BuildUser("P-1001");

            var result = _service.IsAuthorized(user, "P-1002");

            result.Should().BeFalse();
        }

        [Fact]
        public void IsAuthorized_ShouldReturnFalse_WhenClaimIsMissing()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // no PartnerId claim

            var act = () => _service.IsAuthorized(user, "P-1001");

            act.Should().Throw<UnauthorizedAccessException>()
                .WithMessage("Unauthorized");
        }

        [Fact]
        public void IsAuthorized_ShouldReturnFalse_WhenPartnerIdIsEmpty()
        {
            var user = BuildUser("P-1001");

            var result = _service.IsAuthorized(user, "");

            result.Should().BeFalse();
        }
    }
}
