namespace BankingApplication.DTOs
{
    public class TransactionResponse
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
        public string ReferenceNumber { get; set; }
    }
}