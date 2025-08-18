using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service quản lý doanh thu và ví tiền của TourCompany
    /// ENHANCED: Now works with booking-level revenue hold system
    /// Revenue is now stored in TourBooking.RevenueHold instead of TourCompany.RevenueHold
    /// UPDATED: Tour companies now receive 80% (10% platform commission + 10% VAT)
    /// </summary>
    public class TourRevenueService : ITourRevenueService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Commission rate for tour companies (10% - platform fee)
        private const decimal PLATFORM_COMMISSION_RATE = 0.10m;
        
        // VAT rate (10% - tax fee) 
        private const decimal VAT_RATE = 0.10m;
        
        // Total deduction rate (20% = 10% platform + 10% VAT, so tour company gets 80%)
        private const decimal TOTAL_DEDUCTION_RATE = PLATFORM_COMMISSION_RATE + VAT_RATE;

        public TourRevenueService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Thêm tiền vào revenue hold sau khi booking thành công
        /// UPDATED: Keep 100% of payment amount in revenue hold, commission and VAT deducted only at transfer time
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

                // Find the specific booking
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Include(b => b.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy booking"
                    };
                }

                // Verify tour company ownership
                if (booking.TourOperation?.TourDetails?.CreatedById != tourCompanyUserId)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 403,
                        Message = "Không có quyền truy cập booking này"
                    };
                }

                // UPDATED: Set full amount in revenue hold (100%), commission and VAT will be deducted later at transfer time
                booking.RevenueHold = amount; // Keep 100% of payment
                booking.UpdatedAt = VietnamTimeZoneUtility.GetVietnamNow();

                await _unitOfWork.TourBookingRepository.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"Đã set revenue hold {amount:N0} VNĐ cho booking (100% số tiền thanh toán, phí hoa hồng và VAT sẽ trừ khi chuyển tiền)",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi thêm revenue hold: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Chuyển tiền từ revenue hold sang wallet (sau 3 ngày từ ngày tour kết thúc)
        /// DEPRECATED: Use TransferBookingRevenueToWalletAsync instead
        /// </summary>
        public async Task<BaseResposeDto> TransferFromHoldToWalletAsync(Guid tourCompanyUserId, decimal amount)
        {
            return new BaseResposeDto
            {
                StatusCode = 400,
                Message = "Method deprecated. Use TransferBookingRevenueToWalletAsync for individual booking transfers."
            };
        }

        /// <summary>
        /// Chuyển tiền từ booking revenue hold sang TourCompany wallet
        /// UPDATED: Transfer revenue from booking to tour company wallet, deducting 20% total (10% commission + 10% VAT) at transfer time
        /// </summary>
        public async Task<BaseResposeDto> TransferBookingRevenueToWalletAsync(Guid bookingId)
        {
            try
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                // Get booking with all required data
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Include(b => b.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                    .Include(b => b.TourSlot)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy booking"
                    };
                }

                // Check if already transferred
                if (booking.RevenueTransferredDate.HasValue)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Revenue từ booking này đã được chuyển trước đó",
                        success = true
                    };
                }

                // Check if booking is confirmed and has revenue hold
                if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Completed)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Chỉ có thể chuyển tiền từ booking đã xác nhận hoặc hoàn thành"
                    };
                }

                if (booking.RevenueHold <= 0)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Booking này không có revenue hold để chuyển"
                    };
                }

                // Check if tour is completed and 3 days have passed
                var tourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? VietnamTimeZoneUtility.GetVietnamNow();
                var transferEligibleDate = tourDate.AddDays(3);
                var currentTime = VietnamTimeZoneUtility.GetVietnamNow();

                if (currentTime < transferEligibleDate)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Chỉ có thể chuyển tiền sau 3 ngày từ ngày tour kết thúc ({transferEligibleDate:dd/MM/yyyy})"
                    };
                }

                // Get tour company
                var tourCompanyUserId = booking.TourOperation?.TourDetails?.CreatedById;
                if (!tourCompanyUserId.HasValue)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Không tìm thấy thông tin tour company"
                    };
                }

                var tourCompany = await _unitOfWork.TourCompanyRepository
                    .GetFirstOrDefaultAsync(tc => tc.UserId == tourCompanyUserId.Value && !tc.IsDeleted);

                if (tourCompany == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy thông tin công ty tour"
                    };
                }

                // UPDATED: Calculate transfer amount by deducting 20% total (10% commission + 10% VAT) from the full revenue hold
                var fullAmount = booking.RevenueHold; // 100% of customer payment
                var platformCommissionAmount = fullAmount * PLATFORM_COMMISSION_RATE; // 10% platform fee
                var vatAmount = fullAmount * VAT_RATE; // 10% VAT fee
                var totalDeductionAmount = fullAmount * TOTAL_DEDUCTION_RATE; // 20% total deduction
                var transferAmount = fullAmount - totalDeductionAmount; // 80% goes to tour company

                // Transfer net amount (after commission and VAT) to tour company wallet
                tourCompany.Wallet += transferAmount;
                tourCompany.UpdatedAt = VietnamTimeZoneUtility.GetVietnamNow();

                // Mark booking as revenue transferred
                booking.RevenueHold = 0; // Clear revenue hold since it's transferred
                booking.RevenueTransferredDate = VietnamTimeZoneUtility.GetVietnamNow();
                booking.UpdatedAt = VietnamTimeZoneUtility.GetVietnamNow();

                await _unitOfWork.TourCompanyRepository.UpdateAsync(tourCompany);
                await _unitOfWork.TourBookingRepository.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"Đã chuyển {transferAmount:N0} VNĐ từ booking {booking.BookingCode} vào ví công ty tour (sau khi trừ {platformCommissionAmount:N0} VNĐ phí hoa hồng và {vatAmount:N0} VNĐ phí VAT)",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi chuyển revenue từ booking: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Trừ tiền từ revenue hold để chuẩn bị hoàn tiền (khi hủy tour)
        /// UPDATED: Now works with booking-level revenue hold
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

                // Find the specific booking
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Include(b => b.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy booking"
                    };
                }

                // Verify tour company ownership
                if (booking.TourOperation?.TourDetails?.CreatedById != tourCompanyUserId)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 403,
                        Message = "Không có quyền truy cập booking này"
                    };
                }

                // Check if booking has enough revenue hold
                if (booking.RevenueHold < amount)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = $"Booking chỉ có {booking.RevenueHold:N0} VNĐ revenue hold, không đủ để refund {amount:N0} VNĐ"
                    };
                }

                // Reduce revenue hold for refund
                booking.RevenueHold -= amount;
                booking.UpdatedAt = VietnamTimeZoneUtility.GetVietnamNow();

                await _unitOfWork.TourBookingRepository.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"Đã trừ {amount:N0} VNĐ từ revenue hold của booking để chuẩn bị hoàn tiền",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi trừ revenue hold: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Lấy thông tin tài chính của TourCompany
        /// UPDATED: Now includes revenue hold from individual bookings
        /// </summary>
        public async Task<DTOs.Response.TourCompany.TourCompanyFinancialInfo?> GetFinancialInfoAsync(Guid tourCompanyUserId)
        {
            try
            {
                var tourCompany = await _unitOfWork.TourCompanyRepository
                    .GetFirstOrDefaultAsync(tc => tc.UserId == tourCompanyUserId && !tc.IsDeleted);

                if (tourCompany == null)
                    return null;

                // Calculate total revenue hold from all confirmed bookings of this tour company
                var totalRevenueHold = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.TourOperation.TourDetails.CreatedById == tourCompanyUserId 
                        && !b.IsDeleted 
                        && b.Status == BookingStatus.Confirmed 
                        && b.RevenueHold > 0
                        && !b.RevenueTransferredDate.HasValue) // Only bookings that haven't been transferred yet
                    .SumAsync(b => b.RevenueHold);

                // Get count of pending transfer bookings (eligible for transfer after 3 days)
                var currentTime = VietnamTimeZoneUtility.GetVietnamNow();
                var eligibleTransferBookings = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.TourOperation.TourDetails.CreatedById == tourCompanyUserId 
                        && !b.IsDeleted 
                        && b.Status == BookingStatus.Confirmed 
                        && b.RevenueHold > 0
                        && !b.RevenueTransferredDate.HasValue
                        && b.TourSlot != null
                        && b.TourSlot.TourDate.ToDateTime(TimeOnly.MinValue).AddDays(3) <= currentTime)
                    .CountAsync();

                return new DTOs.Response.TourCompany.TourCompanyFinancialInfo
                {
                    TourCompanyId = tourCompany.Id,
                    UserId = tourCompany.UserId,
                    CompanyName = tourCompany.CompanyName,
                    Wallet = tourCompany.Wallet,
                    RevenueHold = totalRevenueHold, // Sum from all bookings
                    TotalRevenue = tourCompany.Wallet + totalRevenueHold,
                    EligibleTransferBookings = eligibleTransferBookings,
                    LastUpdated = tourCompany.UpdatedAt ?? tourCompany.CreatedAt
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting financial info: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all bookings with revenue hold that are eligible for transfer (after 3 days)
        /// NEW: Helper method for automated revenue transfer process
        /// </summary>
        public async Task<List<TourBooking>> GetBookingsEligibleForRevenueTransferAsync()
        {
            try
            {
                var currentTime = VietnamTimeZoneUtility.GetVietnamNow();
                
                return await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Include(b => b.TourOperation)
                        .ThenInclude(to => to.TourDetails)
                    .Include(b => b.TourSlot)
                    .Where(b => !b.IsDeleted 
                        && b.Status == BookingStatus.Confirmed 
                        && b.RevenueHold > 0
                        && !b.RevenueTransferredDate.HasValue
                        && b.TourSlot != null
                        && b.TourSlot.TourDate.ToDateTime(TimeOnly.MinValue).AddDays(3) <= currentTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting bookings eligible for revenue transfer: {ex.Message}");
                return new List<TourBooking>();
            }
        }

        /// <summary>
        /// Tạo TourCompany record cho User mới có role Tour Company
        /// </summary>
        public async Task<BaseResposeDto> CreateTourCompanyAsync(Guid userId, string companyName)
        {
            try
            {
                // Check if TourCompany already exists
                var existingTourCompany = await _unitOfWork.TourCompanyRepository
                    .GetFirstOrDefaultAsync(tc => tc.UserId == userId && !tc.IsDeleted);

                if (existingTourCompany != null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "User đã có TourCompany record"
                    };
                }

                // Create new TourCompany
                var tourCompany = new TourCompany
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CompanyName = companyName,
                    Wallet = 0,
                    RevenueHold = 0,
                    IsActive = true,
                    CreatedAt = VietnamTimeZoneUtility.GetVietnamNow(),
                    CreatedById = userId
                };

                await _unitOfWork.TourCompanyRepository.AddAsync(tourCompany);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Tạo TourCompany thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Lỗi khi tạo TourCompany: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Kiểm tra xem User có phải là Tour Company không
        /// </summary>
        public async Task<bool> IsTourCompanyAsync(Guid userId)
        {
            try
            {
                var tourCompany = await _unitOfWork.TourCompanyRepository
                    .GetFirstOrDefaultAsync(tc => tc.UserId == userId && !tc.IsDeleted);
                
                return tourCompany != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if user is tour company: {ex.Message}");
                return false;
            }
        }
    }
}
