using System.Threading.Tasks;
using teste_backend_acesso.Dto;

namespace teste_backend_acesso.Domain.Interfaces
{
    public interface IServiceAccount
    {
        Task<Response> Get();
        Task<Response> Get(string accountNumber);
        Task<Response> TransferFunds(Transfer transfer);
        Task<Response> TransferFunds(string transactionId);
        void ExecuteTransfer(Transfer transfer);
    }
}
