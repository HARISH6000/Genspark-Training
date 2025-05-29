using BankingApplication.Models;
using System.Threading.Tasks;

namespace BankingApplication.Interfaces
{
    public interface IAccountRepository : IRepository<int, Account>
    {
        Task<Account> GetByAccountNumber(string accountNumber);
    }
}