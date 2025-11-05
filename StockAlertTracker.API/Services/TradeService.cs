using AutoMapper;
using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;

namespace StockAlertTracker.API.Services
{
    public class TradeService : ITradeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStockDataService _stockDataService;
        private readonly IMapper _mapper;

        public TradeService(IUnitOfWork unitOfWork, IStockDataService stockDataService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _stockDataService = stockDataService;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<OrderDetailsDto>> PlaceOrderAsync(int userId, OrderRequestDto orderRequest)
        {
            // 1. Get live price from Finnhub
            var priceResponse = await _stockDataService.GetLiveQuoteAsync(orderRequest.Ticker);
            if (!priceResponse.Success)
            {
                return new ServiceResponse<OrderDetailsDto> { Success = false, Message = $"Stock ticker '{orderRequest.Ticker}' not found." };
            }
            var livePrice = priceResponse.Data.CurrentPrice;

            // 2. Handle BUY order
            if (orderRequest.Type == OrderType.BUY)
            {
                return await HandleBuyOrder(userId, orderRequest, livePrice);
            }

            // 3. Handle SELL order
            if (orderRequest.Type == OrderType.SELL)
            {
                return await HandleSellOrder(userId, orderRequest, livePrice);
            }

            return new ServiceResponse<OrderDetailsDto> { Success = false, Message = "Invalid order type." };
        }

        private async Task<ServiceResponse<OrderDetailsDto>> HandleBuyOrder(int userId, OrderRequestDto orderRequest, decimal livePrice)
        {
            var response = new ServiceResponse<OrderDetailsDto>();
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);
            decimal totalCost = orderRequest.Quantity * livePrice;

            // Check if user has enough money
            if (wallet.Balance < totalCost)
            {
                response.Success = false;
                response.Message = "Insufficient funds to place this order.";
                return response;
            }

            // Lock funds
            wallet.Balance -= totalCost;
            _unitOfWork.Wallets.Update(wallet);

            // Log transaction
            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = -totalCost, // Negative for a debit
                Type = WalletTransactionType.TRADE_DEBIT,
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.WalletTransactions.AddAsync(transaction);

            // Create pending order
            var order = new Order
            {
                UserId = userId,
                Ticker = orderRequest.Ticker,
                Quantity = orderRequest.Quantity,
                PricePerShare = livePrice,
                Type = OrderType.BUY,
                Status = OrderStatus.Pending,
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Orders.AddAsync(order);

            await _unitOfWork.CompleteAsync();

            response.Data = _mapper.Map<OrderDetailsDto>(order);
            response.Message = "Buy order placed successfully. Awaiting admin approval.";
            return response;
        }

        private async Task<ServiceResponse<OrderDetailsDto>> HandleSellOrder(int userId, OrderRequestDto orderRequest, decimal livePrice)
        {
            var response = new ServiceResponse<OrderDetailsDto>();
            var holding = await _unitOfWork.PortfolioHoldings.GetHoldingAsync(userId, orderRequest.Ticker);

            // Check if user owns enough shares
            if (holding == null || holding.Quantity < orderRequest.Quantity)
            {
                response.Success = false;
                response.Message = "Insufficient shares to place this order.";
                return response;
            }

            // Lock shares
            holding.Quantity -= orderRequest.Quantity;
            if (holding.Quantity == 0)
            {
                _unitOfWork.PortfolioHoldings.Delete(holding);
            }
            else
            {
                _unitOfWork.PortfolioHoldings.Update(holding);
            }

            // Create pending order
            var order = new Order
            {
                UserId = userId,
                Ticker = orderRequest.Ticker,
                Quantity = orderRequest.Quantity,
                PricePerShare = livePrice, // This is the price they *sell* at
                Type = OrderType.SELL,
                Status = OrderStatus.Pending,
                Timestamp = DateTime.UtcNow
            };
            await _unitOfWork.Orders.AddAsync(order);

            await _unitOfWork.CompleteAsync();

            response.Data = _mapper.Map<OrderDetailsDto>(order);
            response.Message = "Sell order placed successfully. Awaiting admin approval.";
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<OrderDetailsDto>>> GetMyOrdersAsync(int userId)
        {
            var response = new ServiceResponse<IEnumerable<OrderDetailsDto>>();
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
            response.Data = _mapper.Map<IEnumerable<OrderDetailsDto>>(orders);
            return response;
        }
    }
}