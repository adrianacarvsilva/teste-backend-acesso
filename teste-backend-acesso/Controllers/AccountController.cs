using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using teste_backend_acesso.Domain.Interfaces;
using teste_backend_acesso.Dto;

namespace teste_backend_acesso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        protected IServiceAccount _serviceAccount;

        public AccountController(IServiceAccount serviceAccount)
        {
            _serviceAccount = serviceAccount;
        }

        [HttpGet]
        [Route("get-accounts")]
        public async Task<Response> GetAccounts()
        {
            return await _serviceAccount.Get();
        }

        [HttpGet]
        [Route("get-account")]
        public async Task<Response> GetAccount([FromQuery] string accountNumber)
        {
            return await _serviceAccount.Get(accountNumber);
        }

        [HttpPost]
        [Route("fund-transfer")]
        public async Task<Response> TransferFunds([FromBody] Transfer transfer)
        {
            return await _serviceAccount.TransferFunds(transfer);
        }

        [HttpGet]
        [Route("fund-transfer")]
        public async Task<Response> TransferFunds([FromQuery] string transactionId)
        {
            return await _serviceAccount.TransferFunds(transactionId);
        }
    }
}
