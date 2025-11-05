using AutoMapper;
using StockAlertTracker.API.DTOs.Admin;
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
        }
    }
}