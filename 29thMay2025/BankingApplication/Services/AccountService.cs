using BankingApplication.Interfaces;
using BankingApplication.Models;
using BankingApplication.DTOs;
using System.Threading.Tasks;
using System; 

namespace BankingApplication.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<int, Account> _accountRepository;
        private readonly IRepository<int, User> _userRepository;

        public AccountService(IRepository<int, Account> accountRepository, IRepository<int, User> userRepository)
        {
            _accountRepository = accountRepository;
            _userRepository = userRepository;
        }

        public async Task<Account> AddAccount(CreateAccountRequest accountRequest)
        {
            
            var userExists = await _userRepository.Get(accountRequest.UserId);
            if (userExists == null)
            {
                throw new Exception($"User with ID {accountRequest.UserId} does not exist.");
            }

           
            string newAccountNumber = GenerateUniqueAccountNumber();

            
            var newAccount = new Account
            {
                UserId = accountRequest.UserId,
                AccountNumber = newAccountNumber,
                AccountType = accountRequest.AccountType,
                Balance = accountRequest.InitialDeposit,
                IsActive = true 
            };

            
            var addedAccount = await _accountRepository.Add(newAccount);

            return addedAccount;
        }

        private string GenerateUniqueAccountNumber()
        {
            return Guid.NewGuid().ToString().Substring(0, 10).ToUpperInvariant();
        }
    }
}