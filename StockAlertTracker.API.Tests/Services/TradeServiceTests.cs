using AutoMapper;
using Moq;
using StockAlertTracker.API.DTOs.Stock;
using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;
using StockAlertTracker.API.Services;
using System.Linq.Expressions;
using Xunit;

namespace StockAlertTracker.API.Tests.Services
{
    public class TradeServiceTests
    {
        // --- Mocks ---
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IStockDataService> _stockDataServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IWalletRepository> _walletRepoMock;
        private readonly Mock<IPortfolioHoldingRepository> _portfolioRepoMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IWalletTransactionRepository> _walletTransactionRepoMock;

        // --- Service to Test ---
        private readonly ITradeService _tradeService;

        // --- Test Data ---
        private readonly User _testUser;
        private readonly Wallet _testWallet;
        private readonly PortfolioHolding _testHolding;
        private readonly FinnhubQuoteDto _testQuote;

        public TradeServiceTests()
        {
            // Create Mocks
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _stockDataServiceMock = new Mock<IStockDataService>();
            _mapperMock = new Mock<IMapper>();
            _walletRepoMock = new Mock<IWalletRepository>();
            _portfolioRepoMock = new Mock<IPortfolioHoldingRepository>();
            _orderRepoMock = new Mock<IOrderRepository>();
            _walletTransactionRepoMock = new Mock<IWalletTransactionRepository>();

            // Setup the fake UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.Wallets).Returns(_walletRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.PortfolioHoldings).Returns(_portfolioRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.Orders).Returns(_orderRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.WalletTransactions).Returns(_walletTransactionRepoMock.Object);

            // Create an instance of the *real* TradeService
            _tradeService = new TradeService(
                _unitOfWorkMock.Object,
                _stockDataServiceMock.Object,
                _mapperMock.Object
            );

            // Create re-usable test data
            _testUser = new User { Id = 1 };
            _testWallet = new Wallet { Id = 10, UserId = 1, Balance = 10000 };
            _testHolding = new PortfolioHolding { Id = 20, UserId = 1, Ticker = "AAPL", Quantity = 10 };
            _testQuote = new FinnhubQuoteDto { CurrentPrice = 150 };
        }

        [Fact]
        public async Task PlaceOrderAsync_Buy_Should_Succeed_When_FundsAreSufficient()
        {
            // --- ARRANGE ---
            var buyOrderDto = new OrderRequestDto { Ticker = "AAPL", Quantity = 10, Type = OrderType.BUY }; // Cost = 10 * 150 = 1500

            // "Pretend" Finnhub returns a price
            _stockDataServiceMock.Setup(s => s.GetLiveQuoteAsync("AAPL"))
                                 .ReturnsAsync(new ServiceResponse<FinnhubQuoteDto> { Data = _testQuote });

            // "Pretend" to find the user's wallet (with $10,000)
            _walletRepoMock.Setup(repo => repo.GetByUserIdAsync(1)).ReturnsAsync(_testWallet);

            // "Pretend" AutoMapper works
            _mapperMock.Setup(m => m.Map<OrderDetailsDto>(It.IsAny<Order>()))
                       .Returns(new OrderDetailsDto { Ticker = "AAPL", Quantity = 10 });

            // --- ACT ---
            var result = await _tradeService.PlaceOrderAsync(1, buyOrderDto);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.Equal("Buy order placed successfully. Awaiting admin approval.", result.Message);
            Assert.Equal(8500, _testWallet.Balance); // 10,000 - 1,500 = 8,500
            _walletRepoMock.Verify(repo => repo.Update(_testWallet), Times.Once); // Verify wallet was updated
            _orderRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once); // Verify order was created
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once); // Verify changes were saved
        }

        [Fact]
        public async Task PlaceOrderAsync_Buy_Should_Fail_When_FundsAreInsufficient()
        {
            // --- ARRANGE ---
            var buyOrderDto = new OrderRequestDto { Ticker = "AAPL", Quantity = 100, Type = OrderType.BUY }; // Cost = 100 * 150 = 15,000

            _stockDataServiceMock.Setup(s => s.GetLiveQuoteAsync("AAPL"))
                                 .ReturnsAsync(new ServiceResponse<FinnhubQuoteDto> { Data = _testQuote });

            _walletRepoMock.Setup(repo => repo.GetByUserIdAsync(1)).ReturnsAsync(_testWallet); // User only has $10,000

            // --- ACT ---
            var result = await _tradeService.PlaceOrderAsync(1, buyOrderDto);

            // --- ASSERT ---
            Assert.False(result.Success);
            Assert.Equal("Insufficient funds to place this order.", result.Message);
            Assert.Equal(10000, _testWallet.Balance); // Balance should NOT have changed
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never); // Verify nothing was saved
        }

        [Fact]
        public async Task PlaceOrderAsync_Sell_Should_Succeed_When_SharesAreSufficient()
        {
            // --- ARRANGE ---
            var sellOrderDto = new OrderRequestDto { Ticker = "AAPL", Quantity = 5, Type = OrderType.SELL }; // User owns 10

            _stockDataServiceMock.Setup(s => s.GetLiveQuoteAsync("AAPL"))
                                 .ReturnsAsync(new ServiceResponse<FinnhubQuoteDto> { Data = _testQuote });

            // "Pretend" to find the user's holding
            _portfolioRepoMock.Setup(repo => repo.GetHoldingAsync(1, "AAPL")).ReturnsAsync(_testHolding);

            _mapperMock.Setup(m => m.Map<OrderDetailsDto>(It.IsAny<Order>()))
                       .Returns(new OrderDetailsDto { Ticker = "AAPL", Quantity = 5 });

            // --- ACT ---
            var result = await _tradeService.PlaceOrderAsync(1, sellOrderDto);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.Equal("Sell order placed successfully. Awaiting admin approval.", result.Message);
            Assert.Equal(5, _testHolding.Quantity); // 10 - 5 = 5 shares left
            _portfolioRepoMock.Verify(repo => repo.Update(_testHolding), Times.Once); // Verify holding was updated
            _orderRepoMock.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once); // Verify order was created
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once); // Verify changes were saved
        }

        [Fact]
        public async Task PlaceOrderAsync_Sell_Should_Fail_When_SharesAreInsufficient()
        {
            // --- ARRANGE ---
            var sellOrderDto = new OrderRequestDto { Ticker = "AAPL", Quantity = 20, Type = OrderType.SELL }; // User only owns 10

            _stockDataServiceMock.Setup(s => s.GetLiveQuoteAsync("AAPL"))
                                 .ReturnsAsync(new ServiceResponse<FinnhubQuoteDto> { Data = _testQuote });

            _portfolioRepoMock.Setup(repo => repo.GetHoldingAsync(1, "AAPL")).ReturnsAsync(_testHolding);

            // --- ACT ---
            var result = await _tradeService.PlaceOrderAsync(1, sellOrderDto);

            // --- ASSERT ---
            Assert.False(result.Success);
            Assert.Equal("Insufficient shares to place this order.", result.Message);
            Assert.Equal(10, _testHolding.Quantity); // Quantity should NOT have changed
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never); // Verify nothing was saved
        }
    }
}