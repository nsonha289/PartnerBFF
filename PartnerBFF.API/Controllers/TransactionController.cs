using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnerBFF.Application.Interfaces;
using PartnerBFF.Application.Models.Requests;
using PartnerBFF.Application.Models.Responses;
using PartnerBFF.Application.Services;
using PartnerBFF.Infrastructure.Configurations;
using IAuthorizationService = PartnerBFF.Application.Interfaces.IAuthorizationService;

namespace PartnerBFF.API.Controllers
{
    [Route("api/v1/partner/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = AppConstants.API_KEY)]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IAuthorizationService _authorizationService;

        public TransactionController(ITransactionService transactionService, IAuthorizationService authorizationService)
        {
            _transactionService = transactionService;
            _authorizationService = authorizationService;
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> TransactionsAsync(
            [FromBody] TransactionRequest request, 
            CancellationToken cancellationToken)
        {
            if (!_authorizationService.IsAuthorized(User, request.PartnerId))
                return Forbid();

            var status = await _transactionService.ProcessTransaction(request, cancellationToken);
            var response = new TransactionResponse(request.TransactionReference, status.ToString());
            return Ok(response);
        }
    }
}
