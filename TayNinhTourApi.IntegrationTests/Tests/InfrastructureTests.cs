using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using Xunit;

namespace TayNinhTourApi.IntegrationTests.Tests
{
    /// <summary>
    /// Basic infrastructure tests để verify test framework setup
    /// Không cần database connection
    /// </summary>
    public class InfrastructureTests
    {
        [Fact]
        public void Configuration_ShouldLoadTestSettings()
        {
            // Arrange & Act
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var testDatabaseName = configuration["TestSettings:DatabaseName"];

            // Assert
            connectionString.Should().NotBeNullOrEmpty("Connection string should be configured");
            testDatabaseName.Should().NotBeNullOrEmpty("Test database name should be configured");
            connectionString.Should().Contain("TayNinhTourDb_Test", "Should use test database");
        }

        [Fact]
        public void Entities_ShouldBeInstantiable()
        {
            // Arrange & Act
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Test Role",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                PasswordHash = "test-hash",
                Avatar = "avatar.jpg",
                IsVerified = true,
                RoleId = role.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var template = new TourTemplate
            {
                Id = Guid.NewGuid(),
                Title = "Test Template",
                Description = "Test description",
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

            // Assert
            role.Should().NotBeNull();
            role.Name.Should().Be("Test Role");
            role.IsActive.Should().BeTrue();

            user.Should().NotBeNull();
            user.Email.Should().Be("test@example.com");
            user.RoleId.Should().Be(role.Id);

            template.Should().NotBeNull();
            template.Title.Should().Be("Test Template");
            template.Price.Should().Be(500000);
            template.CreatedById.Should().Be(user.Id);
        }

        [Fact]
        public void Enums_ShouldHaveCorrectValues()
        {
            // Arrange & Act & Assert
            var templateTypes = Enum.GetValues<TourTemplateType>();
            templateTypes.Should().Contain(TourTemplateType.Standard);
            templateTypes.Should().Contain(TourTemplateType.Group);
            templateTypes.Should().Contain(TourTemplateType.Premium);

            var scheduleDays = Enum.GetValues<ScheduleDay>();
            scheduleDays.Should().Contain(ScheduleDay.Saturday);
            scheduleDays.Should().Contain(ScheduleDay.Sunday);

            var slotStatuses = Enum.GetValues<TourSlotStatus>();
            slotStatuses.Should().Contain(TourSlotStatus.Available);
            slotStatuses.Should().Contain(TourSlotStatus.FullyBooked);
            slotStatuses.Should().Contain(TourSlotStatus.Cancelled);
        }

        [Fact]
        public void FluentAssertions_ShouldWork()
        {
            // Arrange
            var testString = "Integration Test";
            var testNumber = 42;
            var testDate = DateTime.UtcNow;
            var testGuid = Guid.NewGuid();

            // Act & Assert
            testString.Should().NotBeNullOrEmpty();
            testString.Should().Contain("Integration");
            testString.Should().StartWith("Integration");
            testString.Should().EndWith("Test");

            testNumber.Should().BeGreaterThan(0);
            testNumber.Should().Be(42);

            testDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            testGuid.Should().NotBeEmpty();
        }

        [Fact]
        public void EntityRelationships_ShouldBeConfigurable()
        {
            // Arrange
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
                Name = "Admin User",
                Email = "admin@example.com",
                PhoneNumber = "0123456789",
                PasswordHash = "admin-hash",
                Avatar = "admin-avatar.jpg",
                IsVerified = true,
                RoleId = role.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var template = new TourTemplate
            {
                Id = Guid.NewGuid(),
                Title = "Relationship Test Template",
                Description = "Testing relationships",
                Price = 750000,
                MaxGuests = 25,
                MinGuests = 10,
                Duration = 2,
                StartLocation = "Ho Chi Minh City",
                EndLocation = "Tay Ninh",
                TemplateType = TourTemplateType.Group,
                ScheduleDays = ScheduleDay.Saturday | ScheduleDay.Sunday,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedById = user.Id
            };

            var tourDetail = new TourDetails
            {
                Id = Guid.NewGuid(),
                TourTemplateId = template.Id,
                TimeSlot = new TimeOnly(9, 30),
                Location = "Test Location",
                Description = "Test detail description",
                SortOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var tourSlot = new TourSlot
            {
                Id = Guid.NewGuid(),
                TourTemplateId = template.Id,
                TourDate = DateOnly.FromDateTime(DateTime.Today.AddDays(14)),
                ScheduleDay = ScheduleDay.Saturday,
                Status = TourSlotStatus.Available,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedById = user.Id
            };

            // Act & Assert - Verify relationships can be set
            user.Role = role;
            template.CreatedBy = user;
            tourDetail.TourTemplate = template;
            tourSlot.TourTemplate = template;
            tourSlot.CreatedBy = user;

            // Verify foreign key relationships
            user.RoleId.Should().Be(role.Id);
            template.CreatedById.Should().Be(user.Id);
            tourDetail.TourTemplateId.Should().Be(template.Id);
            tourSlot.TourTemplateId.Should().Be(template.Id);
            tourSlot.CreatedById.Should().Be(user.Id);

            // Verify navigation properties
            user.Role.Should().Be(role);
            template.CreatedBy.Should().Be(user);
            tourDetail.TourTemplate.Should().Be(template);
            tourSlot.TourTemplate.Should().Be(template);
            tourSlot.CreatedBy.Should().Be(user);
        }

        [Fact]
        public void BusinessRules_ShouldBeValidatable()
        {
            // Arrange
            var template = new TourTemplate
            {
                Id = Guid.NewGuid(),
                Title = "Business Rules Test",
                Description = "Testing business rules",
                Price = 1000000,
                MaxGuests = 30,
                MinGuests = 15,
                Duration = 3,
                StartLocation = "Start Location",
                EndLocation = "End Location",
                TemplateType = TourTemplateType.Premium,
                ScheduleDays = ScheduleDay.Saturday | ScheduleDay.Sunday,
                ChildPrice = 750000,
                ChildMaxAge = 12,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedById = Guid.NewGuid()
            };

            // Act & Assert - Business rules validation
            (template.MinGuests <= template.MaxGuests).Should().BeTrue(
                "MinGuests should not exceed MaxGuests");

            (template.ChildPrice <= template.Price).Should().BeTrue(
                "Child price should not exceed adult price");

            template.ChildMaxAge.Should().BeInRange(1, 17,
                "Child max age should be reasonable");

            template.Price.Should().BeGreaterThan(0,
                "Price should be positive");

            template.Duration.Should().BeGreaterThan(0,
                "Duration should be positive");

            // Group tours should have weekend schedule
            if (template.TemplateType == TourTemplateType.Group)
            {
                (template.ScheduleDays & (ScheduleDay.Saturday | ScheduleDay.Sunday))
                    .Should().NotBe((ScheduleDay)0,
                    "Group tours should be available on weekends");
            }
        }

        [Fact]
        public async Task TestFramework_ShouldSupportAsyncOperations()
        {
            // Arrange & Act
            var result = await Task.Run(async () =>
            {
                await Task.Delay(10); // Simulate async operation
                return "Async Test Result";
            });

            // Assert
            result.Should().Be("Async Test Result");
        }
    }
}
