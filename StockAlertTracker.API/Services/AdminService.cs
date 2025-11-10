using AutoMapper;
using Microsoft.EntityFrameworkCore; // For transactions
using StockAlertTracker.API.DTOs.Admin;
using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.DTOs.Wallet;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;
using System.Text.Json;
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
            string htmlBody = await _emailService.GetTemplateHtmlAsync("KycApproved.html");
            htmlBody = htmlBody.Replace("{{UserName}}", user.FirstName);

            _ = _emailService.SendEmailAsync(user.Email, "KYC Approved! You're Ready to Trade.", htmlBody);
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
            string htmlBody = await _emailService.GetTemplateHtmlAsync("KycRejected.html");
            htmlBody = htmlBody.Replace("{{UserName}}", user.FirstName);

            // We can also fire and forget here if we want, but to handle the exceptions properly, we'll await
            _ =  _emailService.SendEmailAsync(user.Email, "Your KYC Application Update", htmlBody);
            // --- End of Email ---

            response.Message = $"KYC for {user.Email} has been rejected.";
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<OrderDetailsDto>>> GetPendingOrdersAsync()
        {
            var response = new ServiceResponse<IEnumerable<OrderDetailsDto>>();
            var orders = await _unitOfWork.Orders.GetPendingOrdersAsync();
            response.Data = _mapper.Map<IEnumerable<OrderDetailsDto>>(orders);
            return response;
        }

        public async Task<ServiceResponse<OrderDetailsDto>> ApproveOrderAsync(int orderId)
        {
            var response = new ServiceResponse<OrderDetailsDto>();
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            var user = await _unitOfWork.Users.GetByIdAsync(order.UserId); // Need user for email

            if (order == null || order.Status != OrderStatus.Pending)
            {
                response.Success = false;
                response.Message = "Order not found or not pending.";
                return response;
            }

            if (order.Type == OrderType.BUY)
            {
                // Find or create portfolio holding
                var holding = await _unitOfWork.PortfolioHoldings.GetHoldingAsync(order.UserId, order.Ticker);
                if (holding == null)
                {
                    holding = new PortfolioHolding
                    {
                        UserId = order.UserId,
                        Ticker = order.Ticker,
                        Quantity = order.Quantity,
                        AverageCostPrice = order.PricePerShare
                    };
                    await _unitOfWork.PortfolioHoldings.AddAsync(holding);
                }
                else
                {
                    // Recalculate average cost price
                    decimal oldCost = holding.AverageCostPrice * holding.Quantity;
                    decimal newCost = order.PricePerShare * order.Quantity;
                    int totalQuantity = holding.Quantity + order.Quantity;
                    holding.AverageCostPrice = (oldCost + newCost) / totalQuantity;
                    holding.Quantity = totalQuantity;
                    _unitOfWork.PortfolioHoldings.Update(holding);
                }
            }
            else // OrderType.SELL
            {
                // Credit money back to user's wallet
                var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(order.UserId);
                decimal creditAmount = order.Quantity * order.PricePerShare;
                wallet.Balance += creditAmount;
                _unitOfWork.Wallets.Update(wallet);

                // Log transaction
                var transaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = creditAmount, // Positive for a credit
                    Type = WalletTransactionType.TRADE_CREDIT,
                    Timestamp = DateTime.UtcNow
                };
                await _unitOfWork.WalletTransactions.AddAsync(transaction);
            }

            // Mark order as approved
            order.Status = OrderStatus.Approved;
            _unitOfWork.Orders.Update(order);

            await _unitOfWork.CompleteAsync();

            // Send confirmation email
            _ = SendTradeConfirmationEmail(user, order);

            response.Data = _mapper.Map<OrderDetailsDto>(order);
            response.Message = $"Order #{order.Id} approved.";
            return response;
        }

        public async Task<ServiceResponse<OrderDetailsDto>> RejectOrderAsync(int orderId)
        {
            var response = new ServiceResponse<OrderDetailsDto>();

            // 1. Get the order (as you suggested)
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order == null || order.Status != OrderStatus.Pending)
            {
                response.Success = false;
                response.Message = "Order not found or not pending.";
                return response;
            }

            // 2. Get the user (as you suggested)
            var user = await _unitOfWork.Users.GetByIdAsync(order.UserId);
            if (user == null)
            {
                response.Success = false;
                response.Message = "Could not find the user associated with this order.";
                return response;
            }

            // 3. Refund the "locked" resources
            if (order.Type == OrderType.BUY)
            {
                // Refund money
                var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(order.UserId);
                decimal refundAmount = order.Quantity * order.PricePerShare;
                wallet.Balance += refundAmount;
                _unitOfWork.Wallets.Update(wallet);

                // Log transaction (as a reversal)
                var transaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = refundAmount, // Positive, as we are giving it back
                    Type = WalletTransactionType.ADJUSTMENT,
                    Timestamp = DateTime.UtcNow
                };
                await _unitOfWork.WalletTransactions.AddAsync(transaction);
            }
            else // OrderType.SELL
            {
                // Refund shares
                var holding = await _unitOfWork.PortfolioHoldings.GetHoldingAsync(order.UserId, order.Ticker);
                if (holding == null)
                {
                    holding = new PortfolioHolding
                    {
                        UserId = order.UserId,
                        Ticker = order.Ticker,
                        Quantity = order.Quantity,
                        AverageCostPrice = 0 // Price is irrelevant, we are just giving shares back
                    };
                    await _unitOfWork.PortfolioHoldings.AddAsync(holding);
                }
                else
                {
                    holding.Quantity += order.Quantity;
                    _unitOfWork.PortfolioHoldings.Update(holding);
                }
            }

            // 4. Update the order status
            order.Status = OrderStatus.Rejected;
            _unitOfWork.Orders.Update(order);

            await _unitOfWork.CompleteAsync();

            // 5. Call the new helper method to send the email
            _ = SendTradeRejectionEmail(user, order);

            response.Data = _mapper.Map<OrderDetailsDto>(order);
            response.Message = $"Order #{order.Id} rejected and resources refunded.";
            return response;
        }

        public async Task<ServiceResponse<AdminStatsDto>> GetPlatformStatsAsync()
        {
            var response = new ServiceResponse<AdminStatsDto>();

            // 1. Get the latest stats record from the DB
            var latestStats = await _unitOfWork.PlatformStats.GetLatestStatsAsync();

            if (latestStats == null)
            {
                // This can happen if the worker hasn't run yet
                // We'll return an empty/default object
                response.Message = "No analytics data found. Please wait for the nightly report to run.";
                response.Data = new AdminStatsDto
                {
                    TotalUsers = 0,
                    ActiveUsers = 0,
                    TopHeldStocks = new List<StockStatDto>(),
                    TopAlertedStocks = new List<StockStatDto>()
                };
                return response;
            }

            // 2. Deserialize the JSON strings back into objects
            var topHeld = JsonSerializer.Deserialize<List<StockStatDto>>(latestStats.TopWishlistedStocks);
            var topAlerted = JsonSerializer.Deserialize<List<StockStatDto>>(latestStats.TopAlertedStocks);

            // 3. Map to our DTO
            response.Data = new AdminStatsDto
            {
                TotalUsers = latestStats.TotalUsers,
                ActiveUsers = latestStats.ActiveUsers,
                TopHeldStocks = topHeld,
                TopAlertedStocks = topAlerted
            };

            return response;
        }

        public async Task<ServiceResponse<IEnumerable<AdminUserListDto>>> GetAllUsersAsync()
        {
            var response = new ServiceResponse<IEnumerable<AdminUserListDto>>();
            var users = await _unitOfWork.Users.FindAsync(u => u.Role == Models.Enums.RoleType.User);
            response.Data = _mapper.Map<IEnumerable<AdminUserListDto>>(users);
            return response;
        }

        public async Task<ServiceResponse<AdminUserDetailsDto>> GetUserByIdAsync(int userId)
        {
            var response = new ServiceResponse<AdminUserDetailsDto>();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.Role == Models.Enums.RoleType.Admin)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            // 1. Map the basic user details
            var userDetailsDto = _mapper.Map<AdminUserDetailsDto>(user);

            // 2. Manually populate the financial data (as requested)
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            var portfolio = await _unitOfWork.PortfolioHoldings.GetHoldingsByUserIdAsync(userId);
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);

            userDetailsDto.Wallet = _mapper.Map<WalletBalanceDto>(wallet);
            userDetailsDto.Portfolio = _mapper.Map<IEnumerable<PortfolioHoldingDto>>(portfolio);
            userDetailsDto.Orders = _mapper.Map<IEnumerable<OrderDetailsDto>>(orders);

            response.Data = userDetailsDto;
            return response;
        }

        public async Task<ServiceResponse<string>> BlockUserAsync(int userId)
        {
            var response = new ServiceResponse<string>();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }
            if (user.Role == RoleType.Admin)
            {
                response.Success = false;
                response.Message = "Cannot block an admin account.";
                return response;
            }

            user.IsActive = false;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            response.Message = $"User {user.Email} has been successfully blocked.";
            return response;
        }

        public async Task<ServiceResponse<string>> UnblockUserAsync(int userId)
        {
            var response = new ServiceResponse<string>();
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            user.IsActive = true;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            response.Message = $"User {user.Email} has been successfully unblocked.";
            return response;
        }

        // --- Helper methods for sending email ---
        private async Task SendTradeConfirmationEmail(User user, Order order)
        {
            string htmlBody = await _emailService.GetTemplateHtmlAsync("TradeConfirmation.html");

            htmlBody = htmlBody.Replace("{{UserName}}", user.FirstName);
            htmlBody = htmlBody.Replace("{{OrderType}}", order.Type.ToString().ToUpper());
            htmlBody = htmlBody.Replace("{{Quantity}}", order.Quantity.ToString());
            htmlBody = htmlBody.Replace("{{Ticker}}", order.Ticker);
            htmlBody = htmlBody.Replace("{{Status}}", order.Status.ToString());
            htmlBody = htmlBody.Replace("{{PricePerShare}}", order.PricePerShare.ToString("F2"));
            htmlBody = htmlBody.Replace("{{TotalValue}}", (order.Quantity * order.PricePerShare).ToString("F2"));
            htmlBody = htmlBody.Replace("{{OrderId}}", order.Id.ToString());

            await _emailService.SendEmailAsync(user.Email, $"Trade Executed: {order.Type} {order.Quantity} {order.Ticker}", htmlBody);
        }

        private async Task SendTradeRejectionEmail(User user, Order order)
        {
            string htmlBody = await _emailService.GetTemplateHtmlAsync("TradeRejected.html");

            htmlBody = htmlBody.Replace("{{UserName}}", user.FirstName);
            htmlBody = htmlBody.Replace("{{OrderType}}", order.Type.ToString().ToUpper());
            htmlBody = htmlBody.Replace("{{Quantity}}", order.Quantity.ToString());
            htmlBody = htmlBody.Replace("{{Ticker}}", order.Ticker);
            htmlBody = htmlBody.Replace("{{OrderId}}", order.Id.ToString());

            await _emailService.SendEmailAsync(user.Email, $"Your Trade Has Been Rejected: {order.Type} {order.Quantity} {order.Ticker}", htmlBody);
        }

    }
}