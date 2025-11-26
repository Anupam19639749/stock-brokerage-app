using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.DTOs.Wallet;

namespace StockAlertTracker.API.DTOs.Admin
{
    public class AdminUserDetailsDto
    {
        // Profile Info
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        // KYC Info
        public string KycStatus { get; set; }
        public string? PanNumber { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIfscCode { get; set; }

        // Tells the frontend if it should try to call the /image endpoint
        public bool HasProfileImage { get; set; }

        // --- Financial Info (as requested) ---
        public WalletBalanceDto? Wallet { get; set; }
        public IEnumerable<PortfolioHoldingDto> Portfolio { get; set; }
        public IEnumerable<OrderDetailsDto> Orders { get; set; }
    }
}