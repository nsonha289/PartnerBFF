using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PartnerBFF.Application.Interfaces
{
    public interface IAuthorizationService
    {
        bool IsAuthorized(ClaimsPrincipal user, string requestedPartnerId);
    }
}
