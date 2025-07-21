using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service quản lý doanh thu và ví tiền của TourCompany
    /// </summary>
    public class TourRevenueService : ITourRevenueService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TourRevenueService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Thêm tiền vào revenue hold sau khi booking thành công
        /// </summary>
        public async Task<BaseResposeDto> AddToRevenueHoldAsync(Guid tourCompanyUserId, decimal amount, Guid bookingId)
        {
            if (amount <= 0)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Số tiền phải lớn hơn 0"
                };
            }

            try
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                var tourCompany = await _unitOfWork.TourCompanyRepository
                    .GetFirstOrDefaultAsync(tc => tc.UserId == tourCompanyUserId && !tc.IsDeleted);

                if (tourCompany == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin công ty tour"
                    };
                }

                tourCompany.RevenueHold += amount;
                tourCompany.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourCompanyRepository.UpdateAsync(tourCompany);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã thêm tiền vào revenue hold thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi thêm tiền vào revenue hold: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Chuyển tiền từ revenue hold sang wallet
        /// </summary>
        public async Task<BaseResposeDto> TransferFromHoldToWalletAsync(Guid tourCompanyUserId, decimal amount)
        {
            if (amount <= 0)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Số tiền phải lớn hơn 0"
                };
            }

            try
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                var tourCompany = await _unitOfWork.TourCompanyRepository
                    .GetFirstOrDefaultAsync(tc => tc.UserId == tourCompanyUserId && !tc.IsDeleted);

                if (tourCompany == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin công ty tour"
                    };
                }

                if (tourCompany.RevenueHold < amount)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Số tiền trong revenue hold không đủ"
                    };
                }

                tourCompany.RevenueHold -= amount;
                tourCompany.Wallet += amount;
                tourCompany.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourCompanyRepository.UpdateAsync(tourCompany);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã chuyển tiền từ revenue hold sang wallet thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi chuyển tiền: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Trừ tiền từ revenue hold để chuẩn bị hoàn tiền
        /// </summary>
        public async Task<BaseResposeDto> RefundFromRevenueHoldAsync(Guid tourCompanyUserId, decimal amount, Guid bookingId)
        {
            if (amount <= 0)
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Số tiền phải lớn hơn 0"
                };
            }

            try
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                var tourCompany = await _unitOfWork.TourCompanyRepository
                    .GetFirstOrDefaultAsync(tc => tc.UserId == tourCompanyUserId && !tc.IsDeleted);

                if (tourCompany == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin công ty tour"
                    };
                }

                if (tourCompany.RevenueHold < amount)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Số tiền trong revenue hold không đủ để hoàn tiền"
                    };
                }

                tourCompany.RevenueHold -= amount;
                tourCompany.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourCompanyRepository.UpdateAsync(tourCompany);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Đã trừ tiền từ revenue hold để hoàn tiền thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi trừ tiền từ revenue hold: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Lấy thông tin tài chính của TourCompany
        /// </summary>
        public async Task<TourCompanyFinancialInfo?> GetFinancialInfoAsync(Guid tourCompanyUserId)
        {
            var tourCompany = await _unitOfWork.TourCompanyRepository
                .GetFirstOrDefaultAsync(tc => tc.UserId == tourCompanyUserId && !tc.IsDeleted);

            if (tourCompany == null)
                return null;

            return new TourCompanyFinancialInfo
            {
                Id = tourCompany.Id,
                UserId = tourCompany.UserId,
                CompanyName = tourCompany.CompanyName,
                Wallet = tourCompany.Wallet,
                RevenueHold = tourCompany.RevenueHold,
                IsActive = tourCompany.IsActive,
                CreatedAt = tourCompany.CreatedAt,
                UpdatedAt = tourCompany.UpdatedAt ?? tourCompany.CreatedAt
            };
        }

        /// <summary>
        /// Tạo TourCompany record cho User mới có role Tour Company
        /// </summary>
        public async Task<BaseResposeDto> CreateTourCompanyAsync(Guid userId, string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                return new BaseResposeDto
                {
                    StatusCode = 400,
                    Message = "Tên công ty không được để trống"
                };
            }

            try
            {
                // Kiểm tra xem User đã có TourCompany chưa
                var existingCompany = await _unitOfWork.TourCompanyRepository
                    .GetFirstOrDefaultAsync(tc => tc.UserId == userId && !tc.IsDeleted);

                if (existingCompany != null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "User đã có thông tin công ty tour"
                    };
                }

                var tourCompany = new TourCompany
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CompanyName = companyName.Trim(),
                    Wallet = 0,
                    RevenueHold = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId
                };

                await _unitOfWork.TourCompanyRepository.AddAsync(tourCompany);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Tạo thông tin công ty tour thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi tạo thông tin công ty tour: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Kiểm tra xem User có phải là Tour Company không
        /// </summary>
        public async Task<bool> IsTourCompanyAsync(Guid userId)
        {
            var tourCompany = await _unitOfWork.TourCompanyRepository
                .GetFirstOrDefaultAsync(tc => tc.UserId == userId && !tc.IsDeleted);

            return tourCompany != null && tourCompany.IsActive;
        }
    }
}
