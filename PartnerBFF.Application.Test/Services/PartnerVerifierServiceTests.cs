using Microsoft.Extensions.Logging;
using NSubstitute;
using PartnerBFF.Application.Models.Responses;
using PartnerBFF.Application.Services;
using PartnerBFF.Application.Test.Helpers;
using PartnerBFF.Infrastructure.Policies;
using System.Net;

namespace PartnerBFF.Application.Test.Services;

public class PartnerVerifierServiceTests
{
    [Fact]
    public async Task VerifyAsync_ShouldReturnTrue_WhenVerificationSucceeds()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK,
            new PartnerVerificationResponse
            {
                PartnerId = "P-1001",
                IsVerified = true,
                VerifiedAt = DateTime.UtcNow
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost")
        };

        var service = new PartnerVerifierService(
            httpClient,
            Substitute.For<ILogger<PartnerVerifierService>>());

        var result = await service.VerifyPartnerAsync("P-1001");

        Assert.True(result);
    }

    [Fact]
    public async Task VerifyAsync_ShouldReturnFalse_WhenVerificationFails()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.ServiceUnavailable);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost")
        };

        var service = new PartnerVerifierService(
            httpClient,
            Substitute.For<ILogger<PartnerVerifierService>>());

        var result = await service.VerifyPartnerAsync("P-1001");

        Assert.False(result);
    }

    [Fact]
    public async Task RetryPolicy_ShouldRetry_OnTimeoutException()
    {
        var callCount = 0;
        var handler = new MockHttpMessageHandler(() =>
        {
            callCount++;
            if (callCount < 3) throw new TimeoutException("Timed out");
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost")
        };

        var policy = PartnerVerifierPolicy.GetRetryPolicy();
        await policy.ExecuteAsync(() =>
            httpClient.GetAsync("/api/mock/verify/P-1001"));

        Assert.Equal(3, callCount); // failed twice, succeeded on 3rd
    }
}
