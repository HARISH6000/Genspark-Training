using BankingApplication.Models;
using BankingApplication.DTOs;
namespace BankingApplication.Interfaces
{
    public interface IAccountService
    {
        public Task<Account> AddAccount(CreateAccountRequest accountRequest);
    }
}