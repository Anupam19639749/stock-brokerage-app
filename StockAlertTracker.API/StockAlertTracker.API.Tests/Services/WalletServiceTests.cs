using AutoMapper;
using Moq;
using StockAlertTracker.API.DTOs.Wallet;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;
using StockAlertTracker.API.Services;
using Xunit;

namespace StockAlertTracker.API.Tests.Services
{
    public class WalletServiceTests
    {
        // --- Mocks ---
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IWalletRepository> _walletRepoMock;
        private readonly Mock<IWalletTransactionRepository> _walletTransactionRepoMock;

        // --- Service to Test ---
        private readonly IWalletService _walletService;

        // --- Test Data ---
        private readonly User _testUser;
        private readonly Wallet _testWallet;
        private readonly AddMoneyRequestDto _addMoneyDto;

        public WalletServiceTests()
        {
            // Create Mocks
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _mapperMock = new Mock<IMapper>();
            _userRepoMock = new Mock<IUserRepository>();
            _walletRepoMock = new Mock<IWalletRepository>();
            _walletTransactionRepoMock = new Mock<IWalletTransactionRepository>();

            // Setup the fake UnitOfWork to return our fake repositories
            _unitOfWorkMock.Setup(uow => uow.Users).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.Wallets).Returns(_walletRepoMock.Object);
            _unitOfWorkMock.Setup(uow => uow.WalletTransactions).Returns(_walletTransactionRepoMock.Object);

            // Create an instance of the *real* WalletService
            _walletService = new WalletService(
                _unitOfWorkMock.Object,
                _passwordServiceMock.Object,
                _mapperMock.Object
            );

            // Create re-usable test data
            _testUser = new User
            {
                Id = 1,
                KycStatus = KycStatus.Approved,
                PasswordHash = new byte[] { 0x1 },
                PasswordSalt = new byte[] { 0x2 }
            };

            _testWallet = new Wallet
            {
                Id = 10,
                UserId = 1,
                Balance = 1000
            };

            _addMoneyDto = new AddMoneyRequestDto
            {
                Amount = 500,
                Password = "correct_password"
            };
        }

        [Fact]
        public async Task AddMoneyAsync_Should_Succeed_When_KycApproved_And_PasswordIsCorrect()
        {
            // --- ARRANGE ---
            // "Pretend" to find the user
            _userRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(_testUser);

            // "Pretend" the password is correct
            _passwordServiceMock.Setup(p => p.VerifyPasswordHash(_addMoneyDto.Password, _testUser.PasswordHash, _testUser.PasswordSalt))
                                .Returns(true);

            // "Pretend" to find the wallet
            _walletRepoMock.Setup(repo => repo.GetByUserIdAsync(1)).ReturnsAsync(_testWallet);

            // "Pretend" AutoMapper works
            _mapperMock.Setup(m => m.Map<WalletBalanceDto>(It.IsAny<Wallet>()))
                       .Returns(new WalletBalanceDto { Balance = 1500 });

            // --- ACT ---
            var result = await _walletService.AddMoneyAsync(1, _addMoneyDto);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.Equal("Funds added successfully.", result.Message);
            Assert.Equal(1500, result.Data.Balance); // Check if the balance was updated
            Assert.Equal(1500, _testWallet.Balance);  // Check the *actual* object

            // Verify we saved the new transaction
            _unitOfWorkMock.Verify(uow => uow.WalletTransactions.AddAsync(It.IsAny<WalletTransaction>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddMoneyAsync_Should_Fail_When_PasswordIsIncorrect()
        {
            // --- ARRANGE ---
            _userRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(_testUser);

            // "Pretend" the password is *incorrect*
            _passwordServiceMock.Setup(p => p.VerifyPasswordHash(_addMoneyDto.Password, _testUser.PasswordHash, _testUser.PasswordSalt))
                                .Returns(false); // <-- The only difference

            // --- ACT ---
            var result = await _walletService.AddMoneyAsync(1, _addMoneyDto);

            // --- ASSERT ---
            Assert.False(result.Success);
            Assert.Equal("Transaction failed. Incorrect password.", result.Message);
            Assert.Equal(1000, _testWallet.Balance); // Balance should NOT have changed

            // Verify we *never* tried to save
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task AddMoneyAsync_Should_Fail_When_KycIsNotApproved()
        {
            // --- ARRANGE ---
            _testUser.KycStatus = KycStatus.Pending; // User's KYC is pending

            _userRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(_testUser);

            // --- ACT ---
            var result = await _walletService.AddMoneyAsync(1, _addMoneyDto);

            // --- ASSERT ---
            Assert.False(result.Success);
            Assert.Equal("KYC must be approved before you can add funds.", result.Message);

            // Verify we *never* tried to check the password or save
            _passwordServiceMock.Verify(p => p.VerifyPasswordHash(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Never);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Never);
        }
    }
}