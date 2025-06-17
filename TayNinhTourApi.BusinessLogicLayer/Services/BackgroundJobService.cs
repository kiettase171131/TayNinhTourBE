using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Background service để xử lý các time-based transitions cho TourGuide invitation workflow
    /// Chạy các jobs định kỳ để expire invitations, transition status, và cancel unassigned tours
    /// </summary>
    public class BackgroundJobService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly TimeSpan _hourlyInterval = TimeSpan.FromHours(1);
        private readonly TimeSpan _dailyInterval = TimeSpan.FromHours(24);

        public BackgroundJobService(
            IServiceProvider serviceProvider,
            ILogger<BackgroundJobService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundJobService started");

            // Start multiple background tasks
            var tasks = new[]
            {
                RunHourlyJobsAsync(stoppingToken),
                RunDailyJobsAsync(stoppingToken)
            };

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("BackgroundJobService stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BackgroundJobService encountered an error");
            }
        }

        /// <summary>
        /// Chạy các jobs mỗi giờ
        /// </summary>
        private async Task RunHourlyJobsAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting hourly jobs execution");

                    using var scope = _serviceProvider.CreateScope();
                    var invitationService = scope.ServiceProvider.GetRequiredService<ITourGuideInvitationService>();

                    // Job 1: Expire expired invitations
                    await ExpireInvitationsJobAsync(invitationService);

                    _logger.LogInformation("Completed hourly jobs execution");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in hourly jobs execution");
                }

                // Wait for next execution
                try
                {
                    await Task.Delay(_hourlyInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Chạy các jobs mỗi ngày
        /// </summary>
        private async Task RunDailyJobsAsync(CancellationToken stoppingToken)
        {
            // Wait for initial delay to avoid running immediately on startup
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting daily jobs execution");

                    using var scope = _serviceProvider.CreateScope();
                    var invitationService = scope.ServiceProvider.GetRequiredService<ITourGuideInvitationService>();

                    // Job 2: Transition to manual selection (after 24 hours)
                    await TransitionToManualSelectionJobAsync(invitationService);

                    // Job 3: Cancel unassigned TourDetails (after 5 days)
                    await CancelUnassignedToursJobAsync(invitationService);

                    _logger.LogInformation("Completed daily jobs execution");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in daily jobs execution");
                }

                // Wait for next execution
                try
                {
                    await Task.Delay(_dailyInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Job để expire các invitations đã hết hạn
        /// </summary>
        private async Task ExpireInvitationsJobAsync(ITourGuideInvitationService invitationService)
        {
            try
            {
                _logger.LogInformation("Running ExpireInvitations job");

                var expiredCount = await invitationService.ExpireExpiredInvitationsAsync();

                if (expiredCount > 0)
                {
                    _logger.LogInformation("Expired {Count} invitations", expiredCount);
                }
                else
                {
                    _logger.LogDebug("No invitations to expire");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExpireInvitations job");
            }
        }

        /// <summary>
        /// Job để transition TourDetails từ Pending sang AwaitingGuideAssignment sau 24 hours
        /// </summary>
        private async Task TransitionToManualSelectionJobAsync(ITourGuideInvitationService invitationService)
        {
            try
            {
                _logger.LogInformation("Running TransitionToManualSelection job");

                var transitionedCount = await invitationService.TransitionToManualSelectionAsync();

                if (transitionedCount > 0)
                {
                    _logger.LogInformation("Transitioned {Count} TourDetails to manual selection", transitionedCount);
                }
                else
                {
                    _logger.LogDebug("No TourDetails to transition");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TransitionToManualSelection job");
            }
        }

        /// <summary>
        /// Job để cancel TourDetails không có guide assignment sau 5 ngày
        /// </summary>
        private async Task CancelUnassignedToursJobAsync(ITourGuideInvitationService invitationService)
        {
            try
            {
                _logger.LogInformation("Running CancelUnassignedTours job");

                var cancelledCount = await invitationService.CancelUnassignedTourDetailsAsync();

                if (cancelledCount > 0)
                {
                    _logger.LogInformation("Cancelled {Count} unassigned TourDetails", cancelledCount);
                }
                else
                {
                    _logger.LogDebug("No TourDetails to cancel");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CancelUnassignedTours job");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundJobService is stopping");
            await base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("BackgroundJobService disposed");
            base.Dispose();
        }
    }
}
