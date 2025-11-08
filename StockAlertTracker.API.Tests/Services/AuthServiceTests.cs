using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using StockAlertTracker.API.DTOs.Auth;
using StockAlertTracker.API.DTOs.User;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Services;
using System.Text;
using Xunit;

namespace StockAlertTracker.API.Tests.Services
{
    public class AuthServiceTests
    {
        // --- Mocks (Fake Dependencies) ---
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IWalletRepository> _walletRepoMock;
        private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepoMock;

        // --- REAL Dependencies ---
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            // Create fakes for most dependencies
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _emailServiceMock = new Mock<IEmailService>();
            _mapperMock = new Mock<IMapper>();
            _userRepoMock = new Mock<IUserRepository>();
            _walletRepoMock = new Mock<IWalletRepository>();
            _passwordResetTokenRepoMock = new Mock<IPasswordResetTokenRepository>(); // <-- THIS LINE WAS MISSING

            // Tell our fake Unit of Work to use our fake repositories
            _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Wallets).Returns(_walletRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.PasswordResetTokens).Returns(_passwordResetTokenRepoMock.Object); // <-- THIS LINE WAS MISSING

            // --- THIS IS THE FIX ---
            // 1. Create a *real* in-memory configuration for the test
            var jwtSettings = new Dictionary<string, string>
            {
                {"Jwt:Issuer", "fake-issuer"},
                {"Jwt:Audience", "fake-audience"},
                {"Jwt:SecretKey", "a-fake-key-that-is-at-least-32-bytes-long"}
            };

            // 2. Build the real IConfiguration object
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(jwtSettings)
                .Build();
            // --- END OF FIX ---


            // Create the *real* AuthService, giving it the fake dependencies
            // AND our new *real* in-memory configuration
            _authService = new AuthService(
                _unitOfWorkMock.Object,
                _passwordServiceMock.Object,
                _emailServiceMock.Object,
                _configuration,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_Should_Succeed_When_Email_Is_Unique()
        {
            // --- ARRANGE ---
            var registerDto = new RegisterUserDto { Email = "test@example.com", Password = "123" };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>()))
                         .ReturnsAsync((User)null);

            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<UserDetailsDto>(It.IsAny<User>()))
                       .Returns(new UserDetailsDto { Email = "test@example.com" });

            _passwordServiceMock.Setup(p => p.CreatePasswordHash(It.IsAny<string>(), out It.Ref<byte[]>.IsAny, out It.Ref<byte[]>.IsAny))
                                .Callback((string p, out byte[] h, out byte[] s) =>
                                {
                                    h = Encoding.UTF8.GetBytes("fake-hash");
                                    s = Encoding.UTF8.GetBytes("fake-salt");
                                });

            // --- ACT ---
            var result = await _authService.RegisterAsync(registerDto);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.NotNull(result.Data.Token);
            Assert.Equal("User registered successfully.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_Should_Succeed_When_Credentials_Are_Valid()
        {
            // --- ARRANGE ---
            var loginDto = new LoginDto { Email = "test@example.com", Password = "123" };

            var fakeUser = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = Encoding.UTF8.GetBytes("fake-hash"),
                PasswordSalt = Encoding.UTF8.GetBytes("fake-salt")
            };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync("test@example.com"))
                         .ReturnsAsync(fakeUser);

            _passwordServiceMock.Setup(p => p.VerifyPasswordHash(loginDto.Password, fakeUser.PasswordHash, fakeUser.PasswordSalt))
                                .Returns(true);

            _mapperMock.Setup(m => m.Map<UserDetailsDto>(It.IsAny<User>()))
                       .Returns(new UserDetailsDto { Id = 1, Email = "test@example.com" });

            // --- ACT ---
            var result = await _authService.LoginAsync(loginDto);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.NotNull(result.Data.Token);
            Assert.Equal("Login successful.", result.Message);
        }


        [Fact]
        public async Task ForgotPasswordAsync_Should_Succeed_And_SendEmail_When_User_Exists()
        {
            // --- ARRANGE ---
            // 1. Define our input
            var email = "test@example.com";

            // 2. Create a fake user that "exists"
            var fakeUser = new User
            {
                Id = 1,
                Email = email,
                FirstName = "Test"
            };

            // 3. Setup our mocks

            // "Pretend" to find the user
            _userRepoMock.Setup(repo => repo.GetByEmailAsync(email))
                         .ReturnsAsync(fakeUser);

            // "Pretend" to read the HTML email template (now mocking the interface)
            _emailServiceMock.Setup(s => s.GetTemplateHtmlAsync("PasswordReset.html"))
                             .ReturnsAsync("<p>{{Code}}</p>");

            // "Pretend" to save the new token
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);


            // --- ACT ---
            // 4. Call the real method
            var result = await _authService.ForgotPasswordAsync(email);

            // --- ASSERT ---
            // 5. Check the results
            Assert.True(result.Success);

            // Verify that we *tried* to add a token to the database
            _unitOfWorkMock.Verify(uow => uow.PasswordResetTokens.AddAsync(It.IsAny<PasswordResetToken>()), Times.Once);

            // Verify that we *tried* to send an email
            _emailServiceMock.Verify(s => s.SendEmailAsync(
                It.Is<string>(e => e == email),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_Should_Succeed_When_Token_Is_Valid()
        {
            // --- ARRANGE ---
            // 1. Define our input
            var resetDto = new ResetPasswordDto
            {
                Email = "test@example.com",
                Code = "123456", // This is the raw code
                NewPassword = "NewPassword13"
            };

            // 2. Create a fake user that "exists"
            var fakeUser = new User
            {
                Id = 1,
                Email = "test@example.com",
            };

            // 3. Hash the raw code to mimic what's in the DB
            // We call the public ComputeSha256Hash method from our service
            var tokenHash = (_authService as AuthService).ComputeSha256Hash(resetDto.Code);

            // 4. Create a fake token that "exists" in the DB
            var fakeToken = new PasswordResetToken
            {
                Id = 1,
                UserId = 1,
                TokenHash = tokenHash, // The hashed code
                ExpiryDate = DateTime.UtcNow.AddMinutes(5), // Not expired
                IsUsed = false
            };

            // 5. Setup our mocks

            // "Pretend" to find the user
            _userRepoMock.Setup(repo => repo.GetByEmailAsync(resetDto.Email))
                         .ReturnsAsync(fakeUser);

            // "Pretend" to find the token by its hash
            _unitOfWorkMock.Setup(uow => uow.PasswordResetTokens.GetByTokenHashAsync(tokenHash))
                         .ReturnsAsync(fakeToken);

            // "Pretend" our PasswordService works
            _passwordServiceMock.Setup(p => p.CreatePasswordHash(It.IsAny<string>(), out It.Ref<byte[]>.IsAny, out It.Ref<byte[]>.IsAny))
                                .Callback((string p, out byte[] h, out byte[] s) =>
                                {
                                    h = Encoding.UTF8.GetBytes("new-fake-hash");
                                    s = Encoding.UTF8.GetBytes("new-fake-salt");
                                });

            // "Pretend" to save the changes (to User and Token)
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(2); // 2 changes


            // --- ACT ---
            // 6. Call the real method
            var result = await _authService.ResetPasswordAsync(resetDto);

            // --- ASSERT ---
            // 7. Check the results
            Assert.True(result.Success);
            Assert.Equal("Password reset successfully.", result.Message);

            // Verify that the user's password hash was updated
            Assert.Equal(Encoding.UTF8.GetBytes("new-fake-hash"), fakeUser.PasswordHash);

            // Verify that the token was marked as used
            Assert.True(fakeToken.IsUsed);

            // Verify that we saved our changes
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
    }
}