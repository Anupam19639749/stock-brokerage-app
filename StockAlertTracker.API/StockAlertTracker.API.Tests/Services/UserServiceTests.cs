using AutoMapper;
using Moq;
using StockAlertTracker.API.DTOs.User;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;
using StockAlertTracker.API.Services;
using Xunit;

namespace StockAlertTracker.API.Tests.Services
{
    public class UserServiceTests
    {
        // --- Mocks ---
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUserRepository> _userRepoMock;

        // --- Service to Test ---
        private readonly IUserService _userService;

        // --- Test User ---
        private readonly User _testUser;

        public UserServiceTests()
        {
            // Create Mocks
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _userRepoMock = new Mock<IUserRepository>();

            // Setup the fake UnitOfWork to return our fake UserRepository
            _unitOfWorkMock.Setup(uow => uow.Users).Returns(_userRepoMock.Object);

            // Create an instance of the *real* UserService
            _userService = new UserService(_unitOfWorkMock.Object, _mapperMock.Object);

            // Create a re-usable fake user for our tests
            _testUser = new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                KycStatus = KycStatus.Pending,
                ProfileImage = null
            };
        }

        [Fact]
        public async Task GetUserDetailsAsync_Should_ReturnUser_When_UserExists()
        {
            // --- ARRANGE ---
            var userId = 1;
            var userDto = new UserDetailsDto { Id = userId, FirstName = "Test" };

            // "Pretend" to find the user in the database
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                         .ReturnsAsync(_testUser);

            // "Pretend" AutoMapper works
            _mapperMock.Setup(m => m.Map<UserDetailsDto>(_testUser))
                       .Returns(userDto);

            // --- ACT ---
            var result = await _userService.GetUserDetailsAsync(userId);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(userId, result.Data.Id);
        }

        [Fact]
        public async Task SubmitKycAsync_Should_UpdateUser_When_KycIsPending()
        {
            // --- ARRANGE ---
            var userId = 1;
            var kycDto = new KycSubmitDto
            {
                PanNumber = "ABCDE1234F",
                BankName = "Test Bank"
            };

            // "Pretend" to find the user
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                         .ReturnsAsync(_testUser);

            // --- ACT ---
            var result = await _userService.SubmitKycAsync(userId, kycDto);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.Equal("KYC details submitted successfully. Awaiting admin approval.", result.Message);

            // Verify that the user's status was changed to Pending
            Assert.Equal(KycStatus.Pending, _testUser.KycStatus);
            Assert.Equal(kycDto.PanNumber, _testUser.PanNumber);

            // Verify that we *tried* to save the changes
            _unitOfWorkMock.Verify(uow => uow.Users.Update(_testUser), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_Should_UpdateFields_Successfully()
        {
            // --- ARRANGE ---
            var userId = 1;
            var profileDto = new ProfileUpdateDto
            {
                FirstName = "NewFirst",
                LastName = "NewLast",
                Gender = "Male"
            };

            // "Pretend" to find the user
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                         .ReturnsAsync(_testUser);

            // --- ACT ---
            var result = await _userService.UpdateProfileAsync(userId, profileDto);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.Equal("Profile updated successfully.", result.Message);

            // Verify the user object was changed
            Assert.Equal("NewFirst", _testUser.FirstName);
            Assert.Equal("NewLast", _testUser.LastName);
            Assert.Equal("Male", _testUser.Gender);

            _unitOfWorkMock.Verify(uow => uow.Users.Update(_testUser), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileImageAsync_Should_UpdateImage_Successfully()
        {
            // --- ARRANGE ---
            var userId = 1;
            var fakeImageBytes = new byte[] { 0x01, 0x02, 0x03 };
            var fakeContentType = "image/png";

            // "Pretend" to find the user
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                         .ReturnsAsync(_testUser);

            // --- ACT ---
            var result = await _userService.UpdateProfileImageAsync(userId, fakeImageBytes, fakeContentType);

            // --- ASSERT ---
            Assert.True(result.Success);
            Assert.Equal("Profile image updated successfully.", result.Message);

            // Verify the user object was changed
            Assert.Equal(fakeImageBytes, _testUser.ProfileImage);
            Assert.Equal(fakeContentType, _testUser.ProfileImageContentType);

            _unitOfWorkMock.Verify(uow => uow.Users.Update(_testUser), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
    }
}