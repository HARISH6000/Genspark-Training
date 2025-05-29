namespace BankingApplication.DTOs
{
    public class TransferRequest
    {
        public string SourceAccountNumber { get; set; }
        public string TargetAccountNumber { get; set; }
        public decimal Amount { get; set; }
    }
}