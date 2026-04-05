using Microsoft.Extensions.Logging;
using PartnerBFF.Application.Interfaces;
using PartnerBFF.Application.Models.Responses;
using System.Net.Http.Json;

namespace PartnerBFF.Application.Services
{
    public class PartnerVerifierService : IPartnerVerifierService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PartnerVerifierService> _logger;

        public PartnerVerifierService(HttpClient httpClient, ILogger<PartnerVerifierService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"/api/mock/verify/{partnerId}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return false;

                var result = await response.Content
                    .ReadFromJsonAsync<PartnerVerificationResponse>(
                        cancellationToken: cancellationToken);

            return result?.IsVerified ?? false;
        }
    }
}
