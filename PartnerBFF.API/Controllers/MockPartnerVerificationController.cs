using Microsoft.AspNetCore.Mvc;
using PartnerBFF.Application.Models.Responses;

namespace PartnerBFF.API.Controllers
{
    [Route("api/mock")]
    [ApiController]
    public class MockPartnerVerificationController : ControllerBase
    {
        private readonly Random _random = new();

        [HttpGet("verify/{partnerId}")]
        public async Task<IActionResult> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken)
        {
            // Simulate real network latency
            await Task.Delay(100, cancellationToken);

            // 30% chance of timeout
            if (_random.NextDouble() < 0.3)
            {
                // Simulate a real timeout delay before throwing
                await Task.Delay(1000, cancellationToken);
                throw new TimeoutException(
                    $"Partner verification timed out for {partnerId}");
            }

            return Ok(new PartnerVerificationResponse
            {
                PartnerId = partnerId,
                IsVerified = true,
                VerifiedAt = DateTime.UtcNow
            });
        }
    }
}
