namespace BankingApplication.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string TransactionType { get; set; } // "Deposit", "Withdrawal", "Transfer"
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; } // "Completed", "Pending", "Failed"
        public string ReferenceNumber { get; set; }
        public int? TargetAccountId { get; set; }

    }
}