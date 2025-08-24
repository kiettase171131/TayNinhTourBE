using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Background service ?? t? ??ng cleanup các order có status Pending ho?c Cancelled sau 3 ngày
    /// ?ánh d?u IsDeleted = true ?? FE ?n ?i không hi?n th? quá lâu
    /// </summary>
    public class OrderCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Ch?y m?i 6 gi?
        private readonly TimeSpan _hideAfterDays = TimeSpan.FromDays(3); // ?n sau 3 ngày

        public OrderCleanupService(
            IServiceProvider serviceProvider,
            ILogger<OrderCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderCleanupService started - Will hide Pending/Cancelled orders after 3 days");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await HideOldOrdersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while hiding old orders");
                }

                // ??i 6 gi? tr??c khi ch?y l?n ti?p theo
                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("OrderCleanupService stopped");
        }

        /// <summary>
        /// Tìm và ?n các order có status Pending ho?c Cancelled ?ã quá 3 ngày
        /// </summary>
        private async Task HideOldOrdersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(_hideAfterDays);
                
                _logger.LogDebug("Checking for orders to hide - cutoff date: {CutoffDate}", cutoffDate);

                // Tìm các order c?n ?n:
                // - Status = Pending ho?c Cancelled
                // - CreatedAt < cutoffDate (?ã quá 3 ngày)
                // - Ch?a b? ?n (IsDeleted = false)
                var ordersToHide = await unitOfWork.OrderRepository.GetQueryable()
                    .Where(o => (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Cancelled)
                             && o.CreatedAt < cutoffDate
                             && !o.IsDeleted)
                    .ToListAsync();

                if (!ordersToHide.Any())
                {
                    _logger.LogDebug("No orders found that need to be hidden");
                    return;
                }

                _logger.LogInformation("Found {Count} orders to hide (Pending/Cancelled orders older than 3 days)", ordersToHide.Count);

                // S? d?ng execution strategy ?? handle retry logic v?i transactions
                var executionStrategy = unitOfWork.GetExecutionStrategy();

                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await unitOfWork.BeginTransactionAsync();

                    int hiddenCount = 0;
                    foreach (var order in ordersToHide)
                    {
                        try
                        {
                            // ?ánh d?u IsDeleted = true ?? FE ?n ?i
                            order.IsDeleted = true;
                            order.UpdatedAt = DateTime.UtcNow;

                            await unitOfWork.OrderRepository.UpdateAsync(order);
                            hiddenCount++;

                            var daysOld = (DateTime.UtcNow - order.CreatedAt).Days;
                            _logger.LogDebug("Hidden order {OrderId} (PayOS: {PayOsCode}, Status: {Status}, Age: {Days} days)",
                                order.Id, order.PayOsOrderCode ?? "N/A", order.Status, daysOld);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error hiding order {OrderId}", order.Id);
                        }
                    }

                    if (hiddenCount > 0)
                    {
                        await unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();
                        _logger.LogInformation("Successfully hidden {Count} old orders from frontend display", hiddenCount);
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("No orders were successfully hidden");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during order cleanup process");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderCleanupService is stopping");
            await base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("OrderCleanupService disposed");
            base.Dispose();
        }
    }
}