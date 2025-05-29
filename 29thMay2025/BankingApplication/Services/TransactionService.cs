using BankingApplication.Interfaces;
using BankingApplication.Models;
using BankingApplication.DTOs;
using System;
using System.Threading.Tasks;

namespace BankingApplication.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IRepository<int, Transaction> _transactionRepository;

        
        public TransactionService(
            IAccountRepository accountRepository,
            IRepository<int, Transaction> transactionRepository)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<TransactionResponse> Deposit(DepositRequest depositRequest)
        {
            try
            {
                
                var account = await _accountRepository.GetByAccountNumber(depositRequest.AccountNumber);
                if (account == null)
                {
                    throw new Exception($"Account with number {depositRequest.AccountNumber} not found.");
                }
                if (!account.IsActive)
                {
                    throw new Exception($"Account {depositRequest.AccountNumber} is not active.");
                }

                
                account.Balance += depositRequest.Amount;
                
                await _accountRepository.Update(account.Id, account);

                
                var newTransaction = new Transaction
                {
                    AccountId = account.Id,
                    TransactionType = "Deposit",
                    Amount = depositRequest.Amount,
                    TransactionDate = DateTime.UtcNow,
                    Status = "Completed",
                    ReferenceNumber = Guid.NewGuid().ToString(),
                    TargetAccountId = null
                };
                
                await _transactionRepository.Add(newTransaction);

                return MapToTransactionResponse(newTransaction);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error during deposit: {ex.Message}");
                
                return new TransactionResponse
                {
                    Status = "Failed",
                    TransactionType = "Deposit",
                    Amount = depositRequest.Amount,
                    TransactionDate = DateTime.UtcNow,
                    ReferenceNumber = Guid.NewGuid().ToString(),
                };
            }
        }

        public async Task<TransactionResponse> Withdraw(WithdrawalRequest withdrawalRequest)
        {
            try
            {
                
                var account = await _accountRepository.GetByAccountNumber(withdrawalRequest.AccountNumber);
                if (account == null)
                {
                    throw new Exception($"Account with number {withdrawalRequest.AccountNumber} not found.");
                }
                if (!account.IsActive)
                {
                    throw new Exception($"Account {withdrawalRequest.AccountNumber} is not active.");
                }

                
                if (account.Balance < withdrawalRequest.Amount)
                {
                    throw new Exception("Insufficient funds.");
                }

                
                account.Balance -= withdrawalRequest.Amount;
                await _accountRepository.Update(account.Id, account);

                
                var newTransaction = new Transaction
                {
                    AccountId = account.Id,
                    TransactionType = "Withdrawal",
                    Amount = withdrawalRequest.Amount,
                    TransactionDate = DateTime.UtcNow,
                    Status = "Completed",
                    ReferenceNumber = Guid.NewGuid().ToString(),
                    TargetAccountId = null
                };
                await _transactionRepository.Add(newTransaction);

                return MapToTransactionResponse(newTransaction);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during withdrawal: {ex.Message}");
                return new TransactionResponse
                {
                    Status = "Failed",
                    TransactionType = "Withdrawal",
                    Amount = withdrawalRequest.Amount,
                    TransactionDate = DateTime.UtcNow,
                    ReferenceNumber = Guid.NewGuid().ToString(),
                };
            }
        }

        public async Task<TransactionResponse> Transfer(TransferRequest transferRequest)
        {

            try
            {
                
                var sourceAccount = await _accountRepository.GetByAccountNumber(transferRequest.SourceAccountNumber);
                var targetAccount = await _accountRepository.GetByAccountNumber(transferRequest.TargetAccountNumber);

                if (sourceAccount == null)
                {
                    throw new Exception($"Source account {transferRequest.SourceAccountNumber} not found.");
                }
                if (!sourceAccount.IsActive)
                {
                    throw new Exception($"Source account {transferRequest.SourceAccountNumber} is not active.");
                }
                if (targetAccount == null)
                {
                    throw new Exception($"Target account {transferRequest.TargetAccountNumber} not found.");
                }
                if (!targetAccount.IsActive)
                {
                    throw new Exception($"Target account {transferRequest.TargetAccountNumber} is not active.");
                }

                if (sourceAccount.Id == targetAccount.Id)
                {
                    throw new Exception("Cannot transfer money to the same account.");
                }

                
                if (sourceAccount.Balance < transferRequest.Amount)
                {
                    throw new Exception("Insufficient funds in source account.");
                }

                
                sourceAccount.Balance -= transferRequest.Amount;
                targetAccount.Balance += transferRequest.Amount;

                
                await _accountRepository.Update(sourceAccount.Id, sourceAccount);
                await _accountRepository.Update(targetAccount.Id, targetAccount);

                
                var newTransaction = new Transaction
                {
                    AccountId = sourceAccount.Id,
                    TransactionType = "Transfer",
                    Amount = transferRequest.Amount,
                    TransactionDate = DateTime.UtcNow,
                    Status = "Completed",
                    ReferenceNumber = Guid.NewGuid().ToString(),
                    TargetAccountId = targetAccount.Id
                };
                await _transactionRepository.Add(newTransaction);

                return MapToTransactionResponse(newTransaction);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during transfer: {ex.Message}");
                
                return new TransactionResponse
                {
                    Status = "Failed",
                    TransactionType = "Transfer",
                    Amount = transferRequest.Amount,
                    TransactionDate = DateTime.UtcNow,
                    ReferenceNumber = Guid.NewGuid().ToString(),
                };
            }
        }

        private TransactionResponse MapToTransactionResponse(Transaction transaction)
        {
            return new TransactionResponse
            {
                TransactionId = transaction.Id,
                TransactionType = transaction.TransactionType,
                Amount = transaction.Amount,
                TransactionDate = transaction.TransactionDate,
                Status = transaction.Status,
                ReferenceNumber = transaction.ReferenceNumber
            };
        }
    }
}