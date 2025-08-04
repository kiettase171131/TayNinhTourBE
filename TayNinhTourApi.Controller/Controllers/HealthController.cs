using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.BusinessLogicLayer.Utilities;

namespace TayNinhTourApi.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly TayNinhTouApiDbContext _context;

        public HealthController(TayNinhTouApiDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Simple health check endpoint - no DB required
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                Status = "OK",
                Message = "API is running",
                Timestamp = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }

        /// <summary>
        /// Database connection health check
        /// </summary>
        [HttpGet("db")]
        public async Task<IActionResult> DatabaseHealth()
        {
            try
            {
                // Simple DB connectivity test
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Try a simple query
                    var userCount = await _context.Users.CountAsync();
                    
                    return Ok(new
                    {
                        Status = "OK",
                        Message = "Database connection successful",
                        UserCount = userCount,
                        Timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(503, new
                    {
                        Status = "ERROR",
                        Message = "Cannot connect to database",
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    Status = "ERROR",
                    Message = "Database error",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Full system health check
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> SystemStatus()
        {
            var result = new
            {
                API = new
                {
                    Status = "OK",
                    Version = "1.0.0",
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                },
                Database = new { Status = "CHECKING" },
                Timestamp = DateTime.UtcNow
            };

            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var dbResult = new
                {
                    API = result.API,
                    Database = new
                    {
                        Status = canConnect ? "OK" : "ERROR",
                        CanConnect = canConnect,
                        ConnectionString = _context.Database.GetConnectionString()?.Substring(0, 50) + "..."
                    },
                    Timestamp = result.Timestamp
                };

                return Ok(dbResult);
            }
            catch (Exception ex)
            {
                var errorResult = new
                {
                    API = result.API,
                    Database = new
                    {
                        Status = "ERROR",
                        Error = ex.Message
                    },
                    Timestamp = result.Timestamp
                };

                return StatusCode(503, errorResult);
            }
        }

        /// <summary>
        /// Test TNDT prefix implementation
        /// </summary>
        [HttpPost("test-tndt")]
        public async Task<IActionResult> TestTndtPrefix()
        {
            try
            {
                // Generate TNDT order code
                var tndtOrderCode = PayOsOrderCodeUtility.GeneratePayOsOrderCode();

                // Create test PaymentTransaction
                var transaction = new PaymentTransaction
                {
                    Id = Guid.NewGuid(),
                    Amount = 100000,
                    Status = PaymentStatus.Pending,
                    Description = "Test TNDT prefix",
                    Gateway = PaymentGateway.PayOS,
                    PayOsOrderCode = tndtOrderCode,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PaymentTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Status = "SUCCESS",
                    Message = "TNDT prefix test completed",
                    TransactionId = transaction.Id,
                    PayOsOrderCode = transaction.PayOsOrderCode,
                    NumericPart = PayOsOrderCodeUtility.ExtractNumericPart(tndtOrderCode),
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "ERROR",
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get recent PaymentTransactions to verify TNDT format
        /// </summary>
        [HttpGet("payment-transactions")]
        public async Task<IActionResult> GetRecentPaymentTransactions()
        {
            try
            {
                var transactions = await _context.PaymentTransactions
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .Select(t => new
                    {
                        t.Id,
                        t.PayOsOrderCode,
                        t.Amount,
                        t.Status,
                        t.Description,
                        t.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Status = "SUCCESS",
                    Message = "Recent PaymentTransactions retrieved",
                    Transactions = transactions,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "ERROR",
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
