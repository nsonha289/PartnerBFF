using Microsoft.AspNetCore.Mvc;
using PartnerBFF.Application.Interfaces;
using PartnerBFF.Application.Models.Requests;
using PartnerBFF.Application.Models.Responses;

namespace PartnerBFF.API.Controllers
{
    [Route("api/v1/partner/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> TransactionsAsync(
            [FromBody] TransactionRequest request, 
            CancellationToken cancellationToken)
        {
            var status = await _transactionService.ProcessTransaction(request, cancellationToken);
            var response = new TransactionResponse(request.TransactionReference, status.ToString());
            return Ok(response);
        }
    }
}
