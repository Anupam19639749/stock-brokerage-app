using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using StockAlertTracker.API.DTOs.Auth;
using StockAlertTracker.API.DTOs.User;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StockAlertTracker.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            IEmailService emailService,
            IConfiguration config,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _emailService = emailService;
            _config = config;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<UserTokenDto>> RegisterAsync(RegisterUserDto registerDto)
        {
            var response = new ServiceResponse<UserTokenDto>();

            if (await _unitOfWork.Users.GetByEmailAsync(registerDto.Email) != null)
            {
                response.Success = false;
                response.Message = "User with this email already exists.";
                return response;
            }

            _passwordService.CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = Models.Enums.RoleType.User,
                KycStatus = Models.Enums.KycStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Create a wallet for the new user
            var wallet = new Wallet
            {
                User = user,
                Balance = 0
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.Wallets.AddAsync(wallet);

            if (await _unitOfWork.CompleteAsync() == 0)
            {
                response.Success = false;
                response.Message = "Failed to create user.";
                return response;
            }

            var userDetails = _mapper.Map<UserDetailsDto>(user);
            response.Data = new UserTokenDto
            {
                Token = CreateToken(user),
                User = userDetails
            };
            response.Message = "User registered successfully.";
            return response;
        }

        public async Task<ServiceResponse<UserTokenDto>> LoginAsync(LoginDto loginDto)
        {
            var response = new ServiceResponse<UserTokenDto>();
            var user = await _unitOfWork.Users.GetByEmailAsync(loginDto.Email);

            if (user == null)
            {
                response.Success = false;
                response.Message = "Invalid email or password.";
                return response;
            }

            if (!_passwordService.VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Invalid email or password.";
                return response;
            }

            if (!user.IsActive)
            {
                response.Success = false;
                response.Message = "Your account has been suspended. Please contact support.";
                return response;
            }

            // Login successful, update last login
            user.LastLogin = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            var userDetails = _mapper.Map<UserDetailsDto>(user);
            response.Data = new UserTokenDto
            {
                Token = CreateToken(user),
                User = userDetails
            };
            response.Message = "Login successful.";
            return response;
        }

        public async Task<ServiceResponse<string>> ForgotPasswordAsync(string email)
        {
            var response = new ServiceResponse<string>();
            var user = await _unitOfWork.Users.GetByEmailAsync(email);

            if (user == null)
            {
                // IMPORTANT: Do not reveal if an email exists
                response.Message = "If an account with this email exists, a password reset code has been sent.";
                return response; // Return success=true to prevent email snooping
            }

            // Generate 6-digit code
            var code = new Random().Next(100000, 999999).ToString();
            var tokenHash = ComputeSha256Hash(code); // We hash the token

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiryDate = DateTime.UtcNow.AddMinutes(10), // 10 minute expiry
                IsUsed = false
            };

            await _unitOfWork.PasswordResetTokens.AddAsync(resetToken);
            await _unitOfWork.CompleteAsync();

            // Send email
            string htmlBody = await ((EmailService)_emailService).GetTemplateHtmlAsync("PasswordReset.html");
            htmlBody = htmlBody.Replace("{{UserName}}", user.FirstName);
            htmlBody = htmlBody.Replace("{{Code}}", code);

            await _emailService.SendEmailAsync(user.Email, "Reset Your Password", htmlBody);

            response.Message = "If an account with this email exists, a password reset code has been sent.";
            return response;
        }

        public async Task<ServiceResponse<string>> ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            var response = new ServiceResponse<string>();
            var user = await _unitOfWork.Users.GetByEmailAsync(resetDto.Email);

            if (user == null)
            {
                response.Success = false;
                response.Message = "Invalid code or email.";
                return response;
            }

            // Hash the code from the DTO
            var tokenHash = ComputeSha256Hash(resetDto.Code);

            // Find the valid token
            var resetToken = await _unitOfWork.PasswordResetTokens.GetByTokenHashAsync(tokenHash);

            if (resetToken == null || resetToken.UserId != user.Id)
            {
                response.Success = false;
                response.Message = "Invalid code or email.";
                return response;
            }

            // Token is valid. Update password
            _passwordService.CreatePasswordHash(resetDto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // Mark token as used
            resetToken.IsUsed = true;

            _unitOfWork.Users.Update(user);
            _unitOfWork.PasswordResetTokens.Update(resetToken);
            await _unitOfWork.CompleteAsync();

            response.Message = "Password reset successfully.";
            return response;
        }


        // --- Helper Methods ---

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Add role to token
            };

            // Get secret key from user-secrets
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Token valid for 7 days
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}