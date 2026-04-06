using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PartnerBFF.Infrastructure.Configurations;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PartnerBFF.API.Authentications
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ApiKeySettings _settings;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ApiKeySettings settings) : base(options, logger, encoder)
        {
            _settings = settings;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 1. Check header exists
            if (!Request.Headers.TryGetValue(AppConstants.API_KEY_HEADER, out var apiKeyValue))
                return Task.FromResult(
                    AuthenticateResult.Fail("Missing X-Api-Key header"));

            var apiKey = apiKeyValue.ToString();

            // 2. Check key is valid
            if (!_settings.Keys.TryGetValue(apiKey, out var partnerId))
                return Task.FromResult(
                    AuthenticateResult.Fail("Invalid API key"));

            // 3. Build claims identity from the partnerId
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, partnerId),
            new Claim("PartnerId", partnerId)
        };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
