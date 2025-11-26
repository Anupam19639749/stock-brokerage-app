namespace StockAlertTracker.API.DTOs.Admin
{
    public class KycRequestDetailsDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PanNumber { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankIfscCode { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}