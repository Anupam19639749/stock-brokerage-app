using AutoMapper;
using StockAlertTracker.API.DTOs.Admin;
using StockAlertTracker.API.DTOs.Alert;
using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.DTOs.User;
using StockAlertTracker.API.DTOs.Wallet;
using StockAlertTracker.API.Models;

namespace StockAlertTracker.API.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Auth Mappings
            CreateMap<User, UserDetailsDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.KycStatus, opt => opt.MapFrom(src => src.KycStatus.ToString()));

            // Wallet Mappings
            CreateMap<Wallet, WalletBalanceDto>()
                .ForMember(dest => dest.WalletId, opt => opt.MapFrom(src => src.Id));

            CreateMap<WalletTransaction, WalletTransactionDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            // Admin Mappings
            CreateMap<User, KycRequestDetailsDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => src.CreatedAt)); // Assuming submission time is creation time

            // Maps our database Order model to the DTO we show the user
            CreateMap<Order, OrderDetailsDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Maps our database PortfolioHolding model to the DTO
            CreateMap<PortfolioHolding, PortfolioHoldingDto>();

            //Alerts
            CreateMap<PriceAlert, AlertDetailsDto>()
                .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Condition.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            //(Admin User Management) ---
            CreateMap<User, AdminUserListDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.KycStatus, opt => opt.MapFrom(src => src.KycStatus.ToString()));

            CreateMap<User, AdminUserDetailsDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.KycStatus, opt => opt.MapFrom(src => src.KycStatus.ToString()))
                .ForMember(dest => dest.HasProfileImage, opt => opt.MapFrom(src => src.ProfileImage != null && src.ProfileImage.Length > 0))
                // Financial info will be populated manually by the service
                .ForMember(dest => dest.Wallet, opt => opt.Ignore())
                .ForMember(dest => dest.Portfolio, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());
        }
    }
}