namespace StockAlertTracker.API.DTOs.Admin
{
    public class AdminUserListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string KycStatus { get; set; }
        public bool IsActive { get; set; } // For the "Blocked" status
    }
}