namespace PartnerBFF.Application.Models.Responses
{
    public class PartnerVerificationResponse
    {
        public string PartnerId { get; set; }
        public bool IsVerified { get; set; }
        public DateTime VerifiedAt { get; set; }
    }
}
