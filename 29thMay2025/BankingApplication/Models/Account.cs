namespace BankingApplication.Models
{
    public class Account
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public User User { get; set; }
    }

}