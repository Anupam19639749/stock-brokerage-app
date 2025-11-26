using AutoMapper;
using StockAlertTracker.API.DTOs.Alert;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Models;
using StockAlertTracker.API.Models.Enums;

namespace StockAlertTracker.API.Services
{
    public class AlertService : IAlertService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AlertService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<AlertDetailsDto>> CreateAlertAsync(int userId, AlertCreateDto alertDto)
        {
            var response = new ServiceResponse<AlertDetailsDto>();

            // Check if user owns this stock
            var holding = await _unitOfWork.PortfolioHoldings.GetHoldingAsync(userId, alertDto.Ticker);
            if (holding == null)
            {
                response.Success = false;
                response.Message = "You can only set alerts for stocks you own in your portfolio.";
                return response;
            }

            // Check for duplicate active alerts
            var existing = await _unitOfWork.PriceAlerts.FindAsync(
                a => a.UserId == userId &&
                a.Ticker == alertDto.Ticker &&
                a.Condition == alertDto.Condition &&
                a.TargetPrice == alertDto.TargetPrice &&
                a.Status == AlertStatus.Active);

            if (existing.Any())
            {
                response.Success = false;
                response.Message = "An identical active alert already exists.";
                return response;
            }

            var alert = new PriceAlert
            {
                UserId = userId,
                Ticker = alertDto.Ticker,
                Condition = alertDto.Condition,
                TargetPrice = alertDto.TargetPrice,
                Status = AlertStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PriceAlerts.AddAsync(alert);
            await _unitOfWork.CompleteAsync();

            response.Data = _mapper.Map<AlertDetailsDto>(alert);
            response.Message = "Alert created successfully.";
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<AlertDetailsDto>>> GetMyAlertsAsync(int userId)
        {
            var response = new ServiceResponse<IEnumerable<AlertDetailsDto>>();
            var alerts = await _unitOfWork.PriceAlerts.FindAsync(a => a.UserId == userId);
            response.Data = _mapper.Map<IEnumerable<AlertDetailsDto>>(alerts.OrderByDescending(a => a.CreatedAt));
            return response;
        }

        public async Task<ServiceResponse<string>> DeleteAlertAsync(int userId, int alertId)
        {
            var response = new ServiceResponse<string>();
            var alert = await _unitOfWork.PriceAlerts.GetByIdAsync(alertId);

            if (alert == null)
            {
                response.Success = false;
                response.Message = "Alert not found.";
                return response;
            }

            // Security check: Make sure the user owns this alert
            if (alert.UserId != userId)
            {
                response.Success = false;
                response.Message = "You are not authorized to delete this alert.";
                return response;
            }

            _unitOfWork.PriceAlerts.Delete(alert);
            await _unitOfWork.CompleteAsync();

            response.Message = "Alert deleted successfully.";
            return response;
        }
    }
}