using BankingApplication.Contexts;
using BankingApplication.Interfaces;
using BankingApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApplication.Repositories
{
    public class AccountRepository : Repository<int, Account>, IAccountRepository
    {
        public AccountRepository(BankContext bankContext) : base(bankContext)
        {
        }

        public override async Task<Account> Get(int key)
        {
            var account = await _bankContext.Accounts.SingleOrDefaultAsync(p => p.Id == key);

            return account ?? throw new Exception("No account with teh given ID");
        }

        public override async Task<IEnumerable<Account>> GetAll()
        {
            var accounts = _bankContext.Accounts;
            if (accounts.Count() == 0)
                throw new Exception("No accounts in the database");
            return (await accounts.ToListAsync());
        }
        
        public async Task<Account> GetByAccountNumber(string accountNumber)
        {
            var account = await _bankContext.Accounts.SingleOrDefaultAsync(a => a.AccountNumber == accountNumber);
            return account; 
        }
    }
}