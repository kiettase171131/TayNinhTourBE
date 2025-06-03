using AutoMapper;
using System.Diagnostics;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Mapping
{
    /// <summary>
    /// Test class để kiểm tra performance của AutoMapper mappings
    /// Chỉ dùng cho development và testing
    /// </summary>
    public static class MappingPerformanceTest
    {
        /// <summary>
        /// Test performance của TourTemplate mapping với large dataset
        /// </summary>
        public static void TestTourTemplateMapping(IMapper mapper, int itemCount = 1000)
        {
            Console.WriteLine($"Testing TourTemplate mapping performance with {itemCount} items...");

            // Tạo test data
            var tourTemplates = GenerateTourTemplates(itemCount);

            // Test mapping performance
            var stopwatch = Stopwatch.StartNew();
            var dtos = mapper.Map<List<TourTemplateDto>>(tourTemplates);
            stopwatch.Stop();

            Console.WriteLine($"Mapped {itemCount} TourTemplates in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Average: {(double)stopwatch.ElapsedMilliseconds / itemCount:F2}ms per item");
        }

        /// <summary>
        /// Test performance của TourDetails mapping với Shop relationships
        /// </summary>
        public static void TestTourDetailsMapping(IMapper mapper, int itemCount = 1000)
        {
            Console.WriteLine($"Testing TourDetails mapping performance with {itemCount} items...");

            // Tạo test data
            var tourDetails = GenerateTourDetails(itemCount);

            // Test mapping performance
            var stopwatch = Stopwatch.StartNew();
            var dtos = mapper.Map<List<TourDetailDto>>(tourDetails);
            stopwatch.Stop();

            Console.WriteLine($"Mapped {itemCount} TourDetails in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Average: {(double)stopwatch.ElapsedMilliseconds / itemCount:F2}ms per item");
        }

        /// <summary>
        /// Test performance của TourOperation mapping với nested relationships
        /// </summary>
        public static void TestTourOperationMapping(IMapper mapper, int itemCount = 1000)
        {
            Console.WriteLine($"Testing TourOperation mapping performance with {itemCount} items...");

            // Tạo test data
            var tourOperations = GenerateTourOperations(itemCount);

            // Test mapping performance
            var stopwatch = Stopwatch.StartNew();
            var dtos = mapper.Map<List<TourOperationDto>>(tourOperations);
            stopwatch.Stop();

            Console.WriteLine($"Mapped {itemCount} TourOperations in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Average: {(double)stopwatch.ElapsedMilliseconds / itemCount:F2}ms per item");
        }

        /// <summary>
        /// Test tất cả mappings
        /// </summary>
        public static void RunAllTests(IMapper mapper, int itemCount = 1000)
        {
            Console.WriteLine("=== AutoMapper Performance Tests ===");
            TestTourTemplateMapping(mapper, itemCount);
            Console.WriteLine();
            TestTourDetailsMapping(mapper, itemCount);
            Console.WriteLine();
            TestTourOperationMapping(mapper, itemCount);
            Console.WriteLine("=== Tests Completed ===");
        }

        #region Test Data Generation

        private static List<TourTemplate> GenerateTourTemplates(int count)
        {
            var templates = new List<TourTemplate>();
            for (int i = 0; i < count; i++)
            {
                templates.Add(new TourTemplate
                {
                    Id = Guid.NewGuid(),
                    Title = $"Tour Template {i}",
                    Description = $"Description for tour template {i}",
                    Price = 100000 + (i * 1000),
                    MaxGuests = 10 + (i % 20),
                    Duration = 8.5m,
                    TemplateType = (TourTemplateType)(i % 5 + 1),
                    ScheduleDays = ScheduleDay.Saturday | ScheduleDay.Sunday,
                    StartLocation = $"Start Location {i}",
                    EndLocation = $"End Location {i}",
                    MinGuests = 2,
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-i),
                    CreatedById = Guid.NewGuid(),
                    Images = GenerateImages(3),
                    TourDetails = GenerateTourDetails(5)
                });
            }
            return templates;
        }

        private static List<TourDetails> GenerateTourDetails(int count)
        {
            var details = new List<TourDetails>();
            for (int i = 0; i < count; i++)
            {
                details.Add(new TourDetails
                {
                    Id = Guid.NewGuid(),
                    TourTemplateId = Guid.NewGuid(),
                    TimeSlot = new TimeOnly(8 + i, 30),
                    Location = $"Location {i}",
                    Description = $"Description for location {i}",
                    SortOrder = i + 1,
                    ShopId = i % 3 == 0 ? Guid.NewGuid() : null,
                    Shop = i % 3 == 0 ? GenerateShop() : null,
                    CreatedAt = DateTime.Now.AddDays(-i)
                });
            }
            return details;
        }

        private static List<TourOperation> GenerateTourOperations(int count)
        {
            var operations = new List<TourOperation>();
            for (int i = 0; i < count; i++)
            {
                operations.Add(new TourOperation
                {
                    Id = Guid.NewGuid(),
                    TourSlotId = Guid.NewGuid(),
                    GuideId = Guid.NewGuid(),
                    Price = 150000 + (i * 1000),
                    MaxGuests = 15 + (i % 10),
                    Description = $"Operation description {i}",
                    Status = (TourOperationStatus)(i % 6 + 1),
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-i),
                    Guide = GenerateUser(),
                    TourSlot = GenerateTourSlot()
                });
            }
            return operations;
        }

        private static List<Image> GenerateImages(int count)
        {
            var images = new List<Image>();
            for (int i = 0; i < count; i++)
            {
                images.Add(new Image
                {
                    Id = Guid.NewGuid(),
                    Url = $"https://example.com/image{i}.jpg",
                    CreatedAt = DateTime.Now
                });
            }
            return images;
        }

        private static Shop GenerateShop()
        {
            return new Shop
            {
                Id = Guid.NewGuid(),
                Name = "Test Shop",
                Description = "Test shop description",
                Location = "Test location",
                PhoneNumber = "0123456789",
                Email = "shop@test.com",
                ShopType = "Souvenir",
                Rating = 4.5m,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedById = Guid.NewGuid()
            };
        }

        private static User GenerateUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "Test Guide",
                Email = "guide@test.com",
                PhoneNumber = "0123456789",
                Avatar = "https://example.com/avatar.jpg",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
        }

        private static TourSlot GenerateTourSlot()
        {
            return new TourSlot
            {
                Id = Guid.NewGuid(),
                TourTemplateId = Guid.NewGuid(),
                TourDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                ScheduleDay = ScheduleDay.Saturday,
                Status = TourSlotStatus.Available,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedById = Guid.NewGuid()
            };
        }

        #endregion
    }
}
