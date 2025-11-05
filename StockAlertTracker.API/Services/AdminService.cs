using AutoMapper;
using StockAlertTracker.API.DTOs.Admin;
using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;

namespace StockAlertTracker.API.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AdminService(IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<IEnumerable<KycRequestDetailsDto>>> GetKycRequestsAsync()
        {
            var response = new ServiceResponse<IEnumerable<KycRequestDetailsDto>>();
            var pendingUsers = await _unitOfWork.Users.FindAsync(u => u.KycStatus == KycStatus.Pending);

            response.Data = _mapper.Map<IEnumerable<KycRequestDetailsDto>>(pendingUsers);
            return response;
        }

        public async Task<ServiceResponse<string>> ApproveKycAsync(int userId)
        {
            var response = new ServiceResponse<string>();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.KycStatus != KycStatus.Pending)
            {
                response.Success = false;
                response.Message = "User not found or not pending KYC.";
                return response;
            }

            user.KycStatus = KycStatus.Approved;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            // --- SEND KYC APPROVED EMAIL (as planned) ---
            string htmlBody = await ((EmailService)_emailService).GetTemplateHtmlAsync("KycApproved.html");
            htmlBody = htmlBody.Replace("{{UserName}}", user.FirstName);

            await _emailService.SendEmailAsync(user.Email, "KYC Approved! You're Ready to Trade.", htmlBody);
            // --- End of Email ---

            response.Message = $"KYC for {user.Email} has been approved.";
            return response;
        }

        public async Task<ServiceResponse<string>> RejectKycAsync(int userId)
        {
            var response = new ServiceResponse<string>();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null || user.KycStatus != KycStatus.Pending)
            {
                response.Success = false;
                response.Message = "User not found or not pending KYC.";
                return response;
            }

            user.KycStatus = KycStatus.Rejected;
            // Clear their submitted data so they can re-enter
            user.PanNumber = null;
            user.BankName = null;
            user.BankAccountNumber = null;
            user.BankIfscCode = null;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            // --- SEND KYC REJECTED EMAIL (as planned) ---
            string htmlBody = await ((EmailService)_emailService).GetTemplateHtmlAsync("KycRejected.html");
            htmlBody = htmlBody.Replace("{{UserName}}", user.FirstName);

            // We can also fire and forget here if we want, but to handle the exceptions properly, we'll await
            await _emailService.SendEmailAsync(user.Email, "Your KYC Application Update", htmlBody);
            // --- End of Email ---

            response.Message = $"KYC for {user.Email} has been rejected.";
            return response;
        }


        // --- We will implement these in Part 3 ---
        public Task<ServiceResponse<IEnumerable<OrderDetailsDto>>> GetPendingOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<OrderDetailsDto>> ApproveOrderAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<OrderDetailsDto>> RejectOrderAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<AdminStatsDto>> GetPlatformStatsAsync()
        {
            throw new NotImplementedException();
        }
    }
}