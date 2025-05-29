using BankingApplication.Contexts;
using BankingApplication.Interfaces;
using BankingApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApplication.Repositories
{
    public  class TransactionRepository : Repository<int, Transaction>
    {
        public TransactionRepository(BankContext bankContext) : base(bankContext)
        {
        }

        public override async Task<Transaction> Get(int key)
        {
            var transaction = await _bankContext.Transactions.SingleOrDefaultAsync(p => p.Id == key);

            return transaction??throw new Exception("No transaction with teh given ID");
        }

        public override async Task<IEnumerable<Transaction>> GetAll()
        {
            var transactions = _bankContext.Transactions;
            if (transactions.Count() == 0)
                throw new Exception("No Transactions in the database");
            return (await transactions.ToListAsync());
        }
    }
}