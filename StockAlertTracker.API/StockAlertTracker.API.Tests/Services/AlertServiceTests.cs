using AutoMapper;
using Moq;
using StockAlertTracker.API.DTOs.Alert;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;
using StockAlertTracker.API.Services;
using System;
using System.Linq.Expressions;
using Xunit;

namespace StockAlertTracker.API.Tests.Services
{
    public class AlertServiceTests
    {
        // --- Mocks ---
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IPortfolioHoldingRepository> _portfolioRepoMock;
        private readonly Mock<IPriceAlertRepository> _alertRepoMock;

        // --- Service to Test ---
        private readonly IAlertService _alertService;

        // --- Test Data ---
        private readonly PortfolioHolding _testHolding;
        private readonly AlertCreateDto _createAlertDto;

        public AlertServiceTests()
        {
            // Create Mocks
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _portfolioRepoMock = new Mock<IPortfolioHoldingRepository>();
            _alertRepoMock = new Mock<IPriceAlertRepository>();

            // Setup the fake UnitOfWork
            _unitOfWorkMock.Setup(uow => uow.PortfolioHoldings).Returns(_portfolioRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.PriceAlerts).Returns(_alertRepoMock.Object);

            // Create an instance of the *real* AlertService
            _alertService = new AlertService(_unitOfWorkMock.Object, _mapperMock.Object);

            // Create re-usable test data
            _testHolding = new PortfolioHolding { Id = 1, UserId = 1, Ticker = "AAPL", Quantity = 10 };
            _createAlertDto = new AlertCreateDto
            {
                Ticker = "AAPL",
                Condition = AlertCondition.ABOVE,
                TargetPrice = 200
            };

            // "Pretend" AutoMapper works
            _mapperMock.Setup(m => m.Map<AlertDetailsDto>(It.IsAny<PriceAlert>()))
                       .Returns((PriceAlert p) => new AlertDetailsDto { Id = p.Id, Ticker = p.Ticker });
        }

        [Fact]
        public async Task CreateAlertAsync_Should_Succeed_When_User_Owns_Stock()
        {
            // --- ARRANGE ---
            var userId = 1;

            // "Pretend" the user *owns* the stock
            _portfolioRepoMock.Setup(repo => repo.GetHoldingAsync(userId, "AAPL"))
                              .ReturnsAsync(_testHolding);

            // "Pretend" no duplicate alert exists
            _alertRepoMock.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PriceAlert, bool>>>()))
                          .ReturnsAsync(new List<PriceAlert>()); // Empty list

            // --- ACT ---
            var result = await _alertService.CreateAlertAsync(userId, _createAlertDto);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.Equal("Alert created successfully.", result.Message);

            // Verify we saved the new alert
            _alertRepoMock.Verify(repo => repo.AddAsync(It.Is<PriceAlert>(
                a => a.Ticker == "AAPL" && a.TargetPrice == 200
            )), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAlertAsync_Should_Fail_When_User_Does_Not_Own_Stock()
        {
            // --- ARRANGE ---
            var userId = 1;

            // "Pretend" the user *does not own* the stock
            _portfolioRepoMock.Setup(repo => repo.GetHoldingAsync(userId, "AAPL"))
                              .ReturnsAsync((PortfolioHolding)null); // <-- The key difference

            // --- ACT ---
            var result = await _alertService.CreateAlertAsync(userId, _createAlertDto);

            // --- ASSERT ---
            Assert.False(result.Success);
            Assert.Equal("You can only set alerts for stocks you own in your portfolio.", result.Message);

            // Verify we *never* tried to save
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateAlertAsync_Should_Fail_When_Duplicate_Alert_Exists()
        {
            // --- ARRANGE ---
            var userId = 1;

            // "Pretend" the user owns the stock
            _portfolioRepoMock.Setup(repo => repo.GetHoldingAsync(userId, "AAPL"))
                              .ReturnsAsync(_testHolding);

            // "Pretend" a duplicate *does* exist
            var duplicateAlert = new PriceAlert { Ticker = "AAPL", Condition = AlertCondition.ABOVE, TargetPrice = 200, Status = AlertStatus.Active };
            _alertRepoMock.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PriceAlert, bool>>>()))
                          .ReturnsAsync(new List<PriceAlert> { duplicateAlert });

            // --- ACT ---
            var result = await _alertService.CreateAlertAsync(userId, _createAlertDto);

            // --- ASSERT ---
            Assert.False(result.Success);
            Assert.Equal("An identical active alert already exists.", result.Message);

            // Verify we *never* tried to save
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
        }
    }
}