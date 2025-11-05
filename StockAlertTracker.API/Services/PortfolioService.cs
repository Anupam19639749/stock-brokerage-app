using AutoMapper;
using StockAlertTracker.API.DTOs.Trade;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;

namespace StockAlertTracker.API.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PortfolioService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<IEnumerable<PortfolioHoldingDto>>> GetPortfolioAsync(int userId)
        {
            var response = new ServiceResponse<IEnumerable<PortfolioHoldingDto>>();
            var holdings = await _unitOfWork.PortfolioHoldings.GetHoldingsByUserIdAsync(userId);
            response.Data = _mapper.Map<IEnumerable<PortfolioHoldingDto>>(holdings);
            return response;
        }
    }
}