using AutoMapper;
using StockAlertTracker.API.DTOs.Wallet;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;

namespace StockAlertTracker.API.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly IMapper _mapper;

        public WalletService(IUnitOfWork unitOfWork, IPasswordService passwordService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<WalletBalanceDto>> GetWalletBalanceAsync(int userId)
        {
            var response = new ServiceResponse<WalletBalanceDto>();
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);

            if (wallet == null)
            {
                response.Success = false;
                response.Message = "Wallet not found for this user.";
                return response;
            }

            response.Data = _mapper.Map<WalletBalanceDto>(wallet);
            return response;
        }

        public async Task<ServiceResponse<WalletBalanceDto>> AddMoneyAsync(int userId, AddMoneyRequestDto addMoneyDto)
        {
            var response = new ServiceResponse<WalletBalanceDto>();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            // 1. Check for KYC
            if (user.KycStatus != KycStatus.Approved)
            {
                response.Success = false;
                response.Message = "KYC must be approved before you can add funds.";
                return response;
            }

            // 2. VERIFY PASSWORD (as planned)
            if (!_passwordService.VerifyPasswordHash(addMoneyDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Transaction failed. Incorrect password.";
                return response;
            }

            // 3. Find wallet and add funds
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            if (wallet == null)
            {
                response.Success = false;
                response.Message = "Wallet not found. Please contact support.";
                return response;
            }

            wallet.Balance += addMoneyDto.Amount;

            // 4. Log the transaction
            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = addMoneyDto.Amount,
                Type = WalletTransactionType.DEPOSIT,
                Timestamp = DateTime.UtcNow
            };

            _unitOfWork.Wallets.Update(wallet);
            await _unitOfWork.WalletTransactions.AddAsync(transaction);
            await _unitOfWork.CompleteAsync();

            response.Data = _mapper.Map<WalletBalanceDto>(wallet);
            response.Message = "Funds added successfully.";
            return response;
        }

        public async Task<IEnumerable<WalletTransactionDto>> GetWalletHistoryAsync(int userId)
        {
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            if (wallet == null)
            {
                return new List<WalletTransactionDto>(); // Return empty list
            }

            var transactions = await _unitOfWork.WalletTransactions.FindAsync(t => t.WalletId == wallet.Id);
            return _mapper.Map<IEnumerable<WalletTransactionDto>>(transactions.OrderByDescending(t => t.Timestamp));
        }
    }
}