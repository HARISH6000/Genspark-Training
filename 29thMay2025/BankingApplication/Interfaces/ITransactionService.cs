using BankingApplication.Models;
using BankingApplication.DTOs;
namespace BankingApplication.Interfaces
{
    public interface ITransactionService
    {
        public Task<TransactionResponse> Deposit(DepositRequest depositRequest);
        public Task<TransactionResponse> Withdraw(WithdrawalRequest withdrawalRequest);
        public Task<TransactionResponse> Transfer(TransferRequest transferRequest);
    }
}