using AutoMapper;
using StockAlertTracker.API.DTOs.User;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;
using System.Security.Claims;

namespace StockAlertTracker.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<UserDetailsDto>> GetUserDetailsAsync(int userId)
        {
            var response = new ServiceResponse<UserDetailsDto>();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            response.Data = _mapper.Map<UserDetailsDto>(user);
            return response;
        }

        public async Task<ServiceResponse<string>> SubmitKycAsync(int userId, KycSubmitDto kycDto)
        {
            var response = new ServiceResponse<string>();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            if (user.KycStatus == KycStatus.Approved)
            {
                response.Success = false;
                response.Message = "KYC is already approved.";
                return response;
            }

            // Update user's KYC fields
            user.PanNumber = kycDto.PanNumber;
            user.BankName = kycDto.BankName;
            user.BankAccountNumber = kycDto.BankAccountNumber;
            user.BankIfscCode = kycDto.BankIfscCode;
            user.KycStatus = KycStatus.Pending; // Set to Pending for admin to review

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            response.Message = "KYC details submitted successfully. Awaiting admin approval.";
            return response;
        }

        public async Task<ServiceResponse<UserDetailsDto>> UpdateProfileAsync(int userId, ProfileUpdateDto profileDto)
        {
            var response = new ServiceResponse<UserDetailsDto>();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            // Update profile fields
            user.FirstName = profileDto.FirstName;
            user.LastName = profileDto.LastName;
            user.PhoneNumber = profileDto.PhoneNumber;
            user.Gender = profileDto.Gender;
            user.DateOfBirth = profileDto.DateOfBirth;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            response.Data = _mapper.Map<UserDetailsDto>(user);
            response.Message = "Profile updated successfully.";
            return response;
        }

        public async Task<ServiceResponse<string>> UpdateProfileImageAsync(int userId, byte[] imageBytes, string contentType)
        {
            var response = new ServiceResponse<string>();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            // Basic validation (e.g., file size)
            if (imageBytes.Length > 2 * 1024 * 1024) // 2 MB limit
            {
                response.Success = false;
                response.Message = "Image is too large. Max 2MB.";
                return response;
            }

            user.ProfileImage = imageBytes;
            user.ProfileImageContentType = contentType;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            response.Data = "Profile image updated successfully."; 
            response.Message = "Profile image updated successfully.";
            return response;
        }
    }
}