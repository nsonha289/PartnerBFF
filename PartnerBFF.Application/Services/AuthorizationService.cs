using PartnerBFF.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Application.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        public bool IsAuthorized(ClaimsPrincipal user, string requestedPartnerId)
        {
            var authenticatedPartnerId = user.FindFirst("PartnerId") ?? throw new UnauthorizedAccessException("Unauthorized");
            return authenticatedPartnerId.Value == requestedPartnerId;
        }
    }
}
