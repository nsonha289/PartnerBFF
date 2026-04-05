namespace PartnerBFF.Application.Interfaces
{
    public interface IPartnerVerifierService
    {
        Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default);                                                                  
    }
}
