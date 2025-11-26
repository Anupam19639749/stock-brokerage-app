using AutoMapper;
using Moq;
using StockAlertTracker.API.DTOs.Admin;
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
    public class AdminServiceTests
    {
        // --- Mocks ---
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IWalletRepository> _walletRepoMock;
        private readonly Mock<IPortfolioHoldingRepository> _portfolioRepoMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IWalletTransactionRepository> _walletTransactionRepoMock;

        // --- Service to Test ---
        private readonly IAdminService _adminService;

        // --- Test Data ---
        private readonly User _testUser;
        private readonly Wallet _testWallet;
        private Order _pendingBuyOrder;
        private Order _pendingSellOrder;
        private PortfolioHolding _testHolding;

        public AdminServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _emailServiceMock = new Mock<IEmailService>();
            _mapperMock = new Mock<IMapper>();
            _userRepoMock = new Mock<IUserRepository>();
            _walletRepoMock = new Mock<IWalletRepository>();
            _portfolioRepoMock = new Mock<IPortfolioHoldingRepository>();
            _orderRepoMock = new Mock<IOrderRepository>();
            _walletTransactionRepoMock = new Mock<IWalletTransactionRepository>();

            _unitOfWorkMock.Setup(uow => uow.Users).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.Wallets).Returns(_walletRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.PortfolioHoldings).Returns(_portfolioRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.Orders).Returns(_orderRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.WalletTransactions).Returns(_walletTransactionRepoMock.Object);

            _adminService = new AdminService(
                _unitOfWorkMock.Object,
                _emailServiceMock.Object,
                _mapperMock.Object
            );

            _testUser = new User { Id = 1, FirstName = "Test", Email = "test@example.com" };
            _testWallet = new Wallet { Id = 10, UserId = 1, Balance = 5000 };
            _testHolding = new PortfolioHolding { Id = 20, UserId = 1, Ticker = "AAPL", Quantity = 10, AverageCostPrice = 100 };
            _pendingBuyOrder = new Order { Id = 1, UserId = 1, Ticker = "MSFT", Quantity = 10, PricePerShare = 200, Type = OrderType.BUY, Status = OrderStatus.Pending };
            _pendingSellOrder = new Order { Id = 2, UserId = 1, Ticker = "AAPL", Quantity = 5, PricePerShare = 150, Type = OrderType.SELL, Status = OrderStatus.Pending };

            _mapperMock.Setup(m => m.Map<OrderDetailsDto>(It.IsAny<Order>()))
                       .Returns((Order o) => new OrderDetailsDto { Id = o.Id, Ticker = o.Ticker });
        }

        // --- THIS IS THE FIX ---
        // A helper to set up the email mock correctly
        private void SetupEmailTemplateMock(string templateName)
        {
            _emailServiceMock.Setup(s => s.GetTemplateHtmlAsync(templateName))
                             .ReturnsAsync("<p>Mock Template</p>");
        }
        // --- END OF FIX ---


        [Fact]
        public async Task ApproveOrderAsync_Buy_Should_AddHolding_And_SendEmail()
        {
            // --- ARRANGE ---
            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(_pendingBuyOrder);
            _userRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(_testUser);
            _portfolioRepoMock.Setup(repo => repo.GetHoldingAsync(1, "MSFT")).ReturnsAsync((PortfolioHolding)null);

            SetupEmailTemplateMock("TradeConfirmation.html"); // <-- Use the helper

            // --- ACT ---
            var result = await _adminService.ApproveOrderAsync(1);

            // --- ASSERT ---
            Assert.True(result.Success);
            _portfolioRepoMock.Verify(repo => repo.AddAsync(It.IsAny<PortfolioHolding>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
            _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ApproveOrderAsync_Sell_Should_CreditWallet_And_SendEmail()
        {
            // --- ARRANGE ---
            _orderRepoMock.Setup(repo => repo.GetByIdAsync(2)).ReturnsAsync(_pendingSellOrder);
            _userRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(_testUser);
            _walletRepoMock.Setup(repo => repo.GetByUserIdAsync(1)).ReturnsAsync(_testWallet);

            SetupEmailTemplateMock("TradeConfirmation.html"); // <-- Use the helper

            // --- ACT ---
            var result = await _adminService.ApproveOrderAsync(2);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.Equal(5750, _testWallet.Balance);
            _walletTransactionRepoMock.Verify(repo => repo.AddAsync(It.Is<WalletTransaction>(t => t.Amount == 750)), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
            _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RejectOrderAsync_Buy_Should_RefundWallet_And_SendEmail()
        {
            // --- ARRANGE ---
            _orderRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(_pendingBuyOrder);
            _userRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(_testUser); // <-- Added this
            _walletRepoMock.Setup(repo => repo.GetByUserIdAsync(1)).ReturnsAsync(_testWallet);

            SetupEmailTemplateMock("TradeRejected.html"); // <-- Use the helper

            // --- ACT ---
            var result = await _adminService.RejectOrderAsync(1);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.Equal(7000, _testWallet.Balance);
            _walletTransactionRepoMock.Verify(repo => repo.AddAsync(It.Is<WalletTransaction>(t => t.Amount == 2000)), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
            _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RejectOrderAsync_Sell_Should_RefundHolding_And_SendEmail()
        {
            // --- ARRANGE ---
            _orderRepoMock.Setup(repo => repo.GetByIdAsync(2)).ReturnsAsync(_pendingSellOrder);
            _userRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(_testUser); // <-- Added this
            _portfolioRepoMock.Setup(repo => repo.GetHoldingAsync(1, "AAPL")).ReturnsAsync(_testHolding);

            SetupEmailTemplateMock("TradeRejected.html"); // <-- Use the helper

            // --- ACT ---
            var result = await _adminService.RejectOrderAsync(2);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.Equal(15, _testHolding.Quantity);
            _portfolioRepoMock.Verify(repo => repo.Update(_testHolding), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
            _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}