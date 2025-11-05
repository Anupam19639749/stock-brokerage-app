namespace StockAlertTracker.API.DTOs.User
{
    public class UserDetailsDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PanNumber { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIfscCode { get; set; }
        public string KycStatus { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}