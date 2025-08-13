using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<Order?> GetByPayOsOrderCodeAsync(string payOsOrderCode)
        {
            if (string.IsNullOrWhiteSpace(payOsOrderCode))
                return null;
                
            var cleanCode = payOsOrderCode.Trim();
            
            // Log for debugging
            Console.WriteLine($"[OrderRepository] Searching for PayOsOrderCode: '{cleanCode}'");
            
            // First, let's check if the order exists at all (including deleted ones)
            var anyOrder = await _context.Orders
                .AnyAsync(o => o.PayOsOrderCode == cleanCode);
            
            if (anyOrder)
            {
                var deletedOrder = await _context.Orders
                    .AnyAsync(o => o.PayOsOrderCode == cleanCode && o.IsDeleted);
                    
                if (deletedOrder)
                {
                    Console.WriteLine($"[OrderRepository] Order with PayOsOrderCode '{cleanCode}' exists but is marked as deleted");
                }
            }
            else
            {
                Console.WriteLine($"[OrderRepository] No order found with PayOsOrderCode '{cleanCode}' in database");
                
                // Try to list similar codes for debugging
                var similarCodes = await _context.Orders
                    .Where(o => o.PayOsOrderCode != null && o.PayOsOrderCode.Contains("TNDT246967"))
                    .Select(o => new { o.PayOsOrderCode, o.IsDeleted })
                    .ToListAsync();
                    
                if (similarCodes.Any())
                {
                    Console.WriteLine($"[OrderRepository] Found {similarCodes.Count} similar order codes:");
                    foreach (var code in similarCodes)
                    {
                        Console.WriteLine($"  - '{code.PayOsOrderCode}' (IsDeleted: {code.IsDeleted})");
                    }
                }
            }
            
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.PayOsOrderCode == cleanCode && !o.IsDeleted);
        }

        public async Task<Order?> GetByPayOsOrderCodeRawSqlAsync(string payOsOrderCode)
        {
            if (string.IsNullOrWhiteSpace(payOsOrderCode))
                return null;
                
            var cleanCode = payOsOrderCode.Trim();
            
            Console.WriteLine($"[OrderRepository - Raw SQL] Searching for PayOsOrderCode: '{cleanCode}'");
            
            // Use raw SQL to bypass any EF Core parameter binding issues
            var sql = @"
                SELECT * FROM Orders 
                WHERE PayOsOrderCode = {0} 
                AND IsDeleted = 0
                LIMIT 1";
            
            var orders = await _context.Orders
                .FromSqlRaw(sql, cleanCode)
                .Include(o => o.OrderDetails)
                .ToListAsync();
            
            var order = orders.FirstOrDefault();
            
            if (order == null)
            {
                Console.WriteLine($"[OrderRepository - Raw SQL] No order found with PayOsOrderCode '{cleanCode}'");
                
                // Try alternative: direct database query to see all PayOsOrderCodes
                var allCodes = await _context.Orders
                    .FromSqlRaw("SELECT * FROM Orders WHERE PayOsOrderCode IS NOT NULL")
                    .Select(o => new { o.Id, o.PayOsOrderCode, o.IsDeleted })
                    .ToListAsync();
                
                Console.WriteLine($"[OrderRepository - Raw SQL] All PayOsOrderCodes in database:");
                foreach (var code in allCodes.Take(10))
                {
                    Console.WriteLine($"  - ID: {code.Id}, Code: '{code.PayOsOrderCode}', IsDeleted: {code.IsDeleted}");
                }
            }
            else
            {
                Console.WriteLine($"[OrderRepository - Raw SQL] Found order with ID: {order.Id}");
            }
            
            return order;
        }
        
        public DbConnection GetDbConnection()
        {
            return _context.Database.GetDbConnection();
        }
    }
}
