namespace BankingApplication.DTOs
{
    public class CreateAccountRequest
    {
        public int UserId { get; set; }
        public string AccountType { get; set; }
        public decimal InitialDeposit { get; set; }
    }
}