using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using Xunit;

namespace TayNinhTourApi.IntegrationTests.Tests
{
    /// <summary>
    /// Simple database tests để verify integration test infrastructure
    /// Test cơ bản database connection và Entity Framework operations
    /// </summary>
    public class SimpleDatabaseTests : IAsyncLifetime
    {
        private TayNinhTouApiDbContext _context = null!;
        private readonly string _connectionString;

        public SimpleDatabaseTests()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json")
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<TayNinhTouApiDbContext>()
                .UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString))
                .Options;

            _context = new TayNinhTouApiDbContext(options);

            // Ensure database exists
            await _context.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task DatabaseConnection_ShouldBeAccessible()
        {
            // Act
            var canConnect = await _context.Database.CanConnectAsync();

            // Assert
            canConnect.Should().BeTrue("Database should be accessible for integration tests");
        }

        [Fact]
        public async Task DatabaseTables_ShouldExist()
        {
            // Act & Assert - Tables should exist (even if empty)
            var act1 = async () => await _context.Users.CountAsync();
            var act2 = async () => await _context.TourTemplates.CountAsync();
            var act3 = async () => await _context.TourSlots.CountAsync();
            var act4 = async () => await _context.TourDetails.CountAsync();
            var act5 = async () => await _context.Shops.CountAsync();

            await act1.Should().NotThrowAsync("Users table should exist");
            await act2.Should().NotThrowAsync("TourTemplates table should exist");
            await act3.Should().NotThrowAsync("TourSlots table should exist");
            await act4.Should().NotThrowAsync("TourDetails table should exist");
            await act5.Should().NotThrowAsync("Shops table should exist");
        }

        [Fact]
        public async Task CreateRole_ShouldWork()
        {
            // Arrange
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Test Role",
                Description = "Test role for integration testing",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Assert
            var savedRole = await _context.Roles.FindAsync(role.Id);
            savedRole.Should().NotBeNull();
            savedRole!.Name.Should().Be("Test Role");

            // Cleanup
            _context.Roles.Remove(savedRole);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateUser_ShouldWork()
        {
            // Arrange - First create a role
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "User Role",
                Description = "Role for users",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                PasswordHash = "test-hash",
                Avatar = "test-avatar.jpg",
                IsVerified = true,
                RoleId = role.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assert
            var savedUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            savedUser.Should().NotBeNull();
            savedUser!.Email.Should().Be("test@example.com");
            savedUser.Role.Should().NotBeNull();
            savedUser.Role.Name.Should().Be("User Role");

            // Cleanup
            _context.Users.Remove(savedUser);
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateTourTemplate_ShouldWork()
        {
            // Arrange - Create dependencies first
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Admin Role",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Template Creator",
                Email = "creator@example.com",
                PhoneNumber = "0123456789",
                PasswordHash = "test-hash",
                Avatar = "avatar.jpg",
                IsVerified = true,
                RoleId = role.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var template = new TourTemplate
            {
                Id = Guid.NewGuid(),
                Title = "Simple Test Template",
                Description = "Test template for integration testing",
                Price = 500000,
                MaxGuests = 20,
                MinGuests = 5,
                Duration = 1,
                StartLocation = "Ho Chi Minh City",
                EndLocation = "Tay Ninh",
                TemplateType = TourTemplateType.Standard,
                ScheduleDays = ScheduleDay.Saturday | ScheduleDay.Sunday,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedById = user.Id
            };

            // Act
            _context.TourTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Assert
            var savedTemplate = await _context.TourTemplates
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == template.Id);

            savedTemplate.Should().NotBeNull();
            savedTemplate!.Title.Should().Be("Simple Test Template");
            savedTemplate.CreatedBy.Should().NotBeNull();
            savedTemplate.CreatedBy.Name.Should().Be("Template Creator");

            // Cleanup
            _context.TourTemplates.Remove(savedTemplate);
            _context.Users.Remove(user);
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateShop_ShouldWork()
        {
            // Arrange - Create dependencies
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Shop Owner Role",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Shop Creator",
                Email = "shop@example.com",
                PhoneNumber = "0123456789",
                PasswordHash = "test-hash",
                Avatar = "avatar.jpg",
                IsVerified = true,
                RoleId = role.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var shop = new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Test Shop",
                Location = "Test Location",
                PhoneNumber = "0987654321",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedById = user.Id
            };

            // Act
            _context.Shops.Add(shop);
            await _context.SaveChangesAsync();

            // Assert
            var savedShop = await _context.Shops
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(s => s.Id == shop.Id);

            savedShop.Should().NotBeNull();
            savedShop!.Name.Should().Be("Test Shop");
            savedShop.CreatedBy.Should().NotBeNull();

            // Cleanup
            _context.Shops.Remove(savedShop);
            _context.Users.Remove(user);
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task EntityFramework_QueryFilters_ShouldWork()
        {
            // Arrange - Create test data
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Filter Test Role",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Filter Test User",
                Email = "filter@example.com",
                PhoneNumber = "0123456789",
                PasswordHash = "test-hash",
                Avatar = "avatar.jpg",
                IsVerified = true,
                RoleId = role.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var activeTemplate = new TourTemplate
            {
                Id = Guid.NewGuid(),
                Title = "Active Template",
                Description = "Active template",
                Price = 500000,
                MaxGuests = 20,
                MinGuests = 5,
                Duration = 1,
                StartLocation = "Start",
                EndLocation = "End",
                TemplateType = TourTemplateType.Standard,
                ScheduleDays = ScheduleDay.Saturday,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedById = user.Id
            };

            var inactiveTemplate = new TourTemplate
            {
                Id = Guid.NewGuid(),
                Title = "Inactive Template",
                Description = "Inactive template",
                Price = 500000,
                MaxGuests = 20,
                MinGuests = 5,
                Duration = 1,
                StartLocation = "Start",
                EndLocation = "End",
                TemplateType = TourTemplateType.Standard,
                ScheduleDays = ScheduleDay.Saturday,
                IsActive = false, // Inactive
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedById = user.Id
            };

            _context.Roles.Add(role);
            _context.Users.Add(user);
            _context.TourTemplates.AddRange(activeTemplate, inactiveTemplate);
            await _context.SaveChangesAsync();

            // Act - Query without IgnoreQueryFilters (should only return active)
            var activeCount = await _context.TourTemplates.CountAsync();

            // Query with IgnoreQueryFilters (should return all)
            var totalCount = await _context.TourTemplates.IgnoreQueryFilters().CountAsync();

            // Assert
            totalCount.Should().BeGreaterThan(activeCount, "IgnoreQueryFilters should return more records");

            // Cleanup
            _context.TourTemplates.RemoveRange(activeTemplate, inactiveTemplate);
            _context.Users.Remove(user);
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }
    }
}
