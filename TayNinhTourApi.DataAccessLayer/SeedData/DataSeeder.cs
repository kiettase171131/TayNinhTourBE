using Microsoft.EntityFrameworkCore;
using System.Linq;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.SeedData
{
    public class DataSeeder
    {
        private readonly TayNinhTouApiDbContext _context;

        public DataSeeder(TayNinhTouApiDbContext context)
        {
            _context = context;
        }

        public async Task SeedDataAsync()
        {

            if (!await _context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new Role
                    {
                        Id = Guid.Parse("b1860226-3a78-4b5e-a332-fae52b3b7e4d"),
                        Name = "Admin",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false,
                        IsActive = true
                    },
                    new Role
                    {
                        Id = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"),
                        Name = "User",
                        Description = "User role",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsActive = true
                    },
                    new Role
                    {
                        Id = Guid.Parse("7840c6b3-eddf-4929-b8de-df2adc1d1a5b"),
                        Name = "Tour Company",
                        Description = "Tour Company role",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsActive = true
                    },
                     new Role
                    {
                        Id = Guid.Parse("a1f3d2c4-5b6e-7890-abcd-1234567890ef"),
                        Name = "Blogger",
                        Description = "Blogger role",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsActive = true
                    },
                     new Role
                    {
                        Id = Guid.Parse("e2f4a6b8-c1d3-4e5f-a7b9-c2d4e6f8a0b2"),
                        Name = "Tour Guide",
                        Description = "Tour Guide role",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsActive = true
                    },
                     new Role
                    {
                        Id = Guid.Parse("f3e5b7c9-d2e4-5f6a-b8ca-d3e5f7a9b1c3"),
                        Name = "Specialty Shop",
                        Description = "Specialty Shop role",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsActive = true
                    },
                };
                _context.Roles.AddRange(roles);
                await _context.SaveChangesAsync();
            }

            if (!await _context.Users.AnyAsync())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Id = Guid.Parse("c9d05465-76fe-4c93-a469-4e9d090da601"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni",
                        Email = "user@gmail.com",
                        PhoneNumber = "0123456789",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"),
                        Name = "User",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                    new User
                    {
                        Id = Guid.Parse("496eaa57-88aa-41bd-8abf-2aefa6cc47de"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni",
                        Email = "admin@gmail.com",
                        PhoneNumber = "0123456789",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("b1860226-3a78-4b5e-a332-fae52b3b7e4d"),
                        Name = "Admin",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("7a5cbc0b-6082-4215-a90a-9c8cb1b7cc5c"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni",
                        Email = "tourcompany@gmail.com",
                        PhoneNumber = "0123456789",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("7840c6b3-eddf-4929-b8de-df2adc1d1a5b"),
                        Name = "Tour Company",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("f2c4ddf0-c112-4ced-ba08-c684689f8fdc"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni",
                        Email = "blogger@gmail.com",
                        PhoneNumber = "0123456789",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("a1f3d2c4-5b6e-7890-abcd-1234567890ef"),
                        Name = "Blogger",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     // Test users for CV file upload testing
                     new User
                     {
                        Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser1@example.com",
                        PhoneNumber = "0987654321",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 1",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f23456789012"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser2@example.com",
                        PhoneNumber = "0987654322",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 2",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },

                     // Additional 10 test users for easy testing
                     new User
                     {
                        Id = Guid.Parse("d4e5f6a7-b8c9-0123-def4-456789012345"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser3@example.com",
                        PhoneNumber = "0987654324",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 3",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("e5f6a7b8-c9d0-1234-efa5-567890123456"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser4@example.com",
                        PhoneNumber = "0987654325",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 4",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("f6a7b8c9-d0e1-2345-fab6-678901234567"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser5@example.com",
                        PhoneNumber = "0987654326",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 5",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("a7b8c9d0-e1f2-3456-abc7-789012345678"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser6@example.com",
                        PhoneNumber = "0987654327",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 6",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("b8c9d0e1-f2a3-4567-bcd8-890123456789"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser7@example.com",
                        PhoneNumber = "0987654328",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 7",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("c9d0e1f2-a3b4-5678-cde9-901234567890"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser8@example.com",
                        PhoneNumber = "0987654329",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 8",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("d0e1f2a3-b4c5-6789-def0-012345678901"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser9@example.com",
                        PhoneNumber = "0987654330",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 9",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("e1f2a3b4-c5d6-7890-efa1-123456789012"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser10@example.com",
                        PhoneNumber = "0987654331",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 10",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("f2a3b4c5-d6e7-8901-fab2-234567890123"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "testuser11@example.com",
                        PhoneNumber = "0987654332",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f0263e28-97d6-48eb-9b7a-ebd9b383a7e7"), // User role
                        Name = "Test User 11",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                     new User
                     {
                        Id = Guid.Parse("a3b4c5d6-e7f8-9012-abc3-345678901234"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "shop@gmail.com",
                        PhoneNumber = "0987654333",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f3e5b7c9-d2e4-5f6a-b8ca-d3e5f7a9b1c3"), // User role
                        Name = "Shop",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    },
                };
                _context.Users.AddRange(users);
                await _context.SaveChangesAsync();
            }

            // Seed additional tour guide application for English Guide
            if (!await _context.TourGuideApplications.AnyAsync(x => x.Email == "englishguide@gmail.com"))
            {
                var englishGuideUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "englishguide@gmail.com");
                if (englishGuideUser != null)
                {
                    var englishGuideApplication = new TourGuideApplication
                    {
                        Id = Guid.NewGuid(),
                        UserId = englishGuideUser.Id,
                        FullName = "English Tour Guide",
                        PhoneNumber = "0987654400",
                        Email = "englishguide@gmail.com",
                        Experience = "5 years of experience as an English-speaking tour guide. Specialized in cultural tours and historical sites. Fluent in English and Vietnamese.",
                        Skills = "Vietnamese,English,History,Culture,Photography", // English skills
                        CurriculumVitae = "https://example.com/cv-english-guide.pdf",
                        Status = TourGuideApplicationStatus.Approved,
                        SubmittedAt = DateTime.UtcNow.AddDays(-30),
                        ProcessedAt = DateTime.UtcNow.AddDays(-25),
                        ProcessedById = Guid.Parse("496eaa57-88aa-41bd-8abf-2aefa6cc47de"), // Admin ID
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        UpdatedAt = DateTime.UtcNow.AddDays(-25),
                        IsDeleted = false,
                        IsActive = true,
                        CreatedById = englishGuideUser.Id
                    };

                    _context.TourGuideApplications.Add(englishGuideApplication);
                    await _context.SaveChangesAsync();
                }
            }

            if (!await _context.Blogs.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var bloggerUserId = Guid.Parse("f2c4ddf0-c112-4ced-ba08-c684689f8fdc");
                var adminId = Guid.Parse("496eaa57-88aa-41bd-8abf-2aefa6cc47de");

                // Tạo các ID blog cố định
                var blogId1 = Guid.Parse("c5de7158-62ed-42f7-8d5d-1cf8d38a7104"); // Blog Núi Bà
                var blogId2 = Guid.Parse("d4b1c5e2-a3f8-47b9-b5c1-d7e5f8a9b0c3"); // Blog Tòa Thánh Cao Đài

                // Blog 1: Núi Bà Đen
                var blog1 = new Blog
                {
                    Id = blogId1,
                    UserId = bloggerUserId,
                    Status = (byte)BlogStatus.Accepted,
                    Title = "Du lịch Núi Bà Đen - Khám phá ngọn núi thiêng của Tây Ninh",
                    Content = "Núi Bà Đen là một trong những điểm du lịch nổi tiếng nhất tại Tây Ninh, thu hút hàng nghìn du khách mỗi năm. Với độ cao 986m so với mực nước biển, đây là ngọn núi cao nhất khu vực Nam Bộ.</p><p>Chuyến thăm Núi Bà Đen của tôi bắt đầu từ sáng sớm. Hệ thống cáp treo hiện đại đưa du khách lên đến đỉnh núi trong vòng 15 phút, tiết kiệm thời gian và sức lực cho những ai không muốn leo bộ.</p><p>Điểm nhấn của Núi Bà Đen là quần thể chùa Bà nổi tiếng linh thiêng. Đền thờ chính nằm ở lưng chừng núi, nơi thờ Bà Đen (Linh Sơn Thánh Mẫu). Không khí nơi đây vô cùng thanh tịnh, với khói hương nghi ngút và tiếng chuông chùa vang vọng.</p><p>Cảnh quan từ đỉnh núi là điều không thể bỏ qua. Từ đây, bạn có thể phóng tầm mắt bao quát toàn bộ thành phố Tây Ninh và xa hơn nữa là biên giới Việt Nam - Campuchia.</p><p>Núi Bà Đen không chỉ có giá trị tâm linh mà còn là nơi có hệ sinh thái đa dạng. Rừng cây xanh tốt là nơi sinh sống của nhiều loài động thực vật quý hiếm.",
                    AuthorName = "Blogger",
                    CommentOfAdmin = "Bài viết chất lượng, đã được phê duyệt",
                    CreatedById = bloggerUserId,
                    UpdatedById = adminId,
                    CreatedAt = now.AddDays(-10),
                    UpdatedAt = now.AddDays(-9),
                    IsActive = true,
                    IsDeleted = false
                };
                _context.Blogs.Add(blog1);
                await _context.SaveChangesAsync(); // Lưu blog trước để lấy ID

                // Thêm ảnh Núi Bà Đen với URL thực tế đã upload
                var blog1Images = new List<BlogImage>
                {
                    new BlogImage
                    {
                        Id = Guid.Parse("6e8de13a-6ecd-4fff-8440-423ba6b2c807"),
                        BlogId = blogId1,
                        Url = "https://res.cloudinary.com/djo6egmpx/image/upload/v1750398424/2_nui_ba_den_tay_ninh_duoc_menh_danh_la_noc_nha_nam_bo_voi_do_cao_986m_941ad4e224_ra6ez8.jpg",
                        CreatedById = bloggerUserId,
                        CreatedAt = now.AddDays(-10),
                        IsActive = true
                    }
                };
                _context.BlogImages.AddRange(blog1Images);
                await _context.SaveChangesAsync();

                // Blog 2: Tòa Thánh Cao Đài
                var blog2 = new Blog
                {
                    Id = blogId2,
                    UserId = bloggerUserId,
                    Status = (byte)BlogStatus.Accepted,
                    Title = "Khám phá Tòa Thánh Cao Đài - Kiến trúc độc đáo của Tây Ninh",
                    Content = "Tòa Thánh Cao Đài tại Tây Ninh là công trình kiến trúc tôn giáo độc đáo bậc nhất Việt Nam. Đây không chỉ là trung tâm tôn giáo của đạo Cao Đài mà còn là điểm đến văn hóa hấp dẫn du khách trong và ngoài nước.</p><p>Kiến trúc của Tòa Thánh là sự kết hợp hài hòa giữa phong cách Đông - Tây. Từ xa, bạn có thể nhìn thấy mái vòm màu xanh dương nổi bật giữa không gian rộng lớn. Công trình được xây dựng từ năm 1933 đến 1955 mới hoàn thành.</p><p>Bước vào bên trong, ấn tượng đầu tiên của tôi là không gian rộng lớn với những cột trụ được trang trí công phu. Điểm nhấn chính là hình ảnh Thiên Nhãn - biểu tượng của đạo Cao Đài - được đặt trang trọng trên bàn thờ chính.</p><p>Mỗi chi tiết trang trí đều mang ý nghĩa tôn giáo sâu sắc, từ những bức tranh minh họa đến các biểu tượng như rồng, phượng, hoa sen... Màu sắc chủ đạo của Tòa Thánh là hồng, xanh, vàng - đại diện cho Tam giáo: Phật, Thánh, Tiên.</p><p>Điều thú vị là du khách có thể tham dự các buổi lễ hằng ngày diễn ra tại đây vào 6h, 12h, 18h và 24h. Tín đồ trong trang phục trắng trang nghiêm làm lễ tạo nên khung cảnh đặc biệt ấn tượng.",
                    AuthorName = "Blogger",
                    CommentOfAdmin = "Bài viết chất lượng cao, đã được phê duyệt",
                    CreatedById = bloggerUserId,
                    UpdatedById = adminId,
                    CreatedAt = now.AddDays(-8),
                    UpdatedAt = now.AddDays(-7),
                    IsActive = true,
                    IsDeleted = false
                };
                _context.Blogs.Add(blog2);
                await _context.SaveChangesAsync();

                // Thêm ảnh Tòa Thánh Cao Đài với URL thực tế đã upload
                var blog2Images = new List<BlogImage>
                {
                    new BlogImage
                    {
                        Id = Guid.Parse("532fe45e-8a72-453e-bfdd-b753cd4f9952"),
                        BlogId = blogId2,
                        Url = "https://res.cloudinary.com/djo6egmpx/image/upload/v1750398428/a873_q4iieg.jpg",
                        CreatedById = bloggerUserId,
                        CreatedAt = now.AddDays(-8),
                        IsActive = true
                    }
                };
                _context.BlogImages.AddRange(blog2Images);
                await _context.SaveChangesAsync();
            }

            // Seed TourCompanies first (required for TourTemplates)
            var tourCompanyId = Guid.NewGuid(); // This will be the TourCompany.Id
            var tourCompanyUserId = Guid.Parse("7a5cbc0b-6082-4215-a90a-9c8cb1b7cc5c"); // Tour company user ID

            if (!await _context.TourCompanies.AnyAsync())
            {
                var now = DateTime.UtcNow;

                var tourCompany = new TourCompany
                {
                    Id = tourCompanyId, // Use predefined ID
                    UserId = tourCompanyUserId,
                    CompanyName = "Tây Ninh Travel Company",
                    Wallet = 100000,
                    RevenueHold = 0,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = now,
                    CreatedById = tourCompanyUserId,
                    UpdatedAt = now,
                    UpdatedById = tourCompanyUserId
                };

                _context.TourCompanies.Add(tourCompany);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Get existing TourCompany ID
                var existingTourCompany = await _context.TourCompanies.FirstAsync();
                tourCompanyId = existingTourCompany.Id;
            }

            // Seed TourTemplates for testing
            if (!await _context.TourTemplates.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var adminId = Guid.Parse("496eaa57-88aa-41bd-8abf-2aefa6cc47de");
                // Use User.Id for CreatedById (now references Users table instead of TourCompanies)

                var tourTemplates = new List<TourTemplate>
                {
                    // Template 1 - Free Scenic Tour (Núi Bà Đen) - Saturday
                    new TourTemplate
                    {
                        Id = Guid.Parse("b740b8a6-716f-41a6-a7e7-f7f9e09d7925"),
                        Title = "Tour Núi Bà Đen - Danh lam thắng cảnh",
                        TemplateType = TourTemplateType.FreeScenic,
                        ScheduleDays = ScheduleDay.Saturday,
                        StartLocation = "TP.HCM - Bến xe Miền Tây",
                        EndLocation = "Núi Bà Đen - Tây Ninh",
                        Month = 6,
                        Year = 2025,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = now.AddDays(-10),
                        CreatedById = tourCompanyUserId,
                        UpdatedAt = now.AddDays(-5),
                        UpdatedById = tourCompanyUserId
                    },

                    // Template 2 - Free Scenic Tour (Tòa Thánh Cao Đài) - Sunday
                    new TourTemplate
                    {
                        Id = Guid.Parse("f0288a60-20b0-457c-af62-68e054e98dac"),
                        Title = "Tour Tòa Thánh Cao Đài - Di tích lịch sử",
                        TemplateType = TourTemplateType.FreeScenic,
                        ScheduleDays = ScheduleDay.Sunday,
                        StartLocation = "TP.HCM - Bến xe Miền Tây",
                        EndLocation = "Tòa Thánh Cao Đài - Tây Ninh",
                        Month = 6,
                        Year = 2025,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = now.AddDays(-8),
                        CreatedById = tourCompanyUserId,
                        UpdatedAt = now.AddDays(-3),
                        UpdatedById = tourCompanyUserId
                    },

                    // Template 3 - Paid Attraction Tour - Saturday
                    new TourTemplate
                    {
                        Id = Guid.Parse("a6683345-e4d4-4273-b1d9-d65542cf0755"),
                        Title = "Tour Khu du lịch sinh thái Tây Ninh",
                        TemplateType = TourTemplateType.PaidAttraction,
                        ScheduleDays = ScheduleDay.Saturday,
                        StartLocation = "TP.HCM - Bến xe Miền Tây",
                        EndLocation = "Khu du lịch sinh thái - Tây Ninh",
                        Month = 7,
                        Year = 2025,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = now.AddDays(-6),
                        CreatedById = tourCompanyUserId,
                        UpdatedAt = now.AddDays(-1),
                        UpdatedById = tourCompanyUserId
                    },

                    // Template 4 - Next month template - Sunday
                    new TourTemplate
                    {
                        Id = Guid.Parse("0009ddda-5f69-407f-9241-b567dde990dc"),
                        Title = "Tour Núi Bà Đen - Tháng tới",
                        TemplateType = TourTemplateType.FreeScenic,
                        ScheduleDays = ScheduleDay.Sunday,
                        StartLocation = "TP.HCM - Bến xe Miền Tây",
                        EndLocation = "Núi Bà Đen - Tây Ninh",
                        Month = 8,
                        Year = 2025,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = now.AddDays(-4),
                        CreatedById = tourCompanyUserId,
                        UpdatedAt = now.AddDays(-2),
                        UpdatedById = tourCompanyUserId
                    }
                };

                await _context.TourTemplates.AddRangeAsync(tourTemplates);
                await _context.SaveChangesAsync();
            }

            // Seed 3 TourGuide Users with different roles first
            var tourGuideUsers = new List<User>();
            var specialtyShopUsers = new List<User>();

            // Create 3 TourGuide Users if they don't exist
            var tourGuideUserIds = new[]
            {
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Guid.Parse("33333333-3333-3333-3333-333333333333")
            };

            var specialtyShopUserIds = new[]
            {
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Guid.Parse("66666666-6666-6666-6666-666666666666")
            };

            // Check and create TourGuide users
            foreach (var userId in tourGuideUserIds)
            {
                if (!await _context.Users.AnyAsync(u => u.Id == userId))
                {
                    var index = Array.IndexOf(tourGuideUserIds, userId) + 1;
                    tourGuideUsers.Add(new User
                    {
                        Id = userId,
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = $"tourguide{index}@example.com",
                        PhoneNumber = $"098765432{index}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("e2f4a6b8-c1d3-4e5f-a7b9-c2d4e6f8a0b2"), // Tour Guide role
                        Name = $"Tour Guide {index}",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    });
                }
            }

            // Check and create SpecialtyShop users
            foreach (var userId in specialtyShopUserIds)
            {
                if (!await _context.Users.AnyAsync(u => u.Id == userId))
                {
                    var index = Array.IndexOf(specialtyShopUserIds, userId) + 1;
                    specialtyShopUsers.Add(new User
                    {
                        Id = userId,
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = $"specialtyshop{index}@example.com",
                        PhoneNumber = $"098765433{index}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("f3e5b7c9-d2e4-5f6a-b8ca-d3e5f7a9b1c3"), // Specialty Shop role
                        Name = $"Specialty Shop {index}",
                        Avatar = "https://static-00.iconduck.com/assets.00/avatar-default-icon-2048x2048-h6w375ur.png",
                        IsActive = true,
                    });
                }
            }

            if (tourGuideUsers.Any() || specialtyShopUsers.Any())
            {
                _context.Users.AddRange(tourGuideUsers.Concat(specialtyShopUsers));
                await _context.SaveChangesAsync();
            }

            // Seed 3 TourGuide Applications with different skills
            if (!await _context.TourGuideApplications.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var adminId = Guid.Parse("496eaa57-88aa-41bd-8abf-2aefa6cc47de");

                // Ensure users exist before creating applications
                var existingTourGuideUsers = await _context.Users
                    .Where(u => tourGuideUserIds.Contains(u.Id))
                    .ToListAsync();

                if (existingTourGuideUsers.Count < 3)
                {
                    // Skip creating applications if users don't exist
                    return;
                }

                var applications = new List<TourGuideApplication>
                {
                    // TourGuide 1 - History Specialist (Approved)
                    new TourGuideApplication
                    {
                        Id = Guid.Parse("d1e2f3a4-b5c6-7890-def1-234567890abc"),
                        UserId = tourGuideUserIds[0],
                        FullName = "Nguyen Van Duc",
                        PhoneNumber = "0987654321",
                        Email = "tourguide1@example.com",
                        Experience = "5 years specializing in historical tours of Cao Dai Temple and Cu Chi Tunnels. Expert in Vietnamese history and Cao Dai religion.",
                        Languages = "Vietnamese", // Legacy field
                        Skills = "History", // ONLY History skill
                        CurriculumVitae = "http://localhost:5267/uploads/cv/2024/12/tourguide1/cv.pdf",
                        CvOriginalFileName = "NguyenVanDuc_CV.pdf",
                        CvFileSize = 2048000, // 2MB
                        CvContentType = "application/pdf",
                        CvFilePath = "uploads/cv/2024/12/tourguide1/cv.pdf",
                        Status = TourGuideApplicationStatus.Approved,
                        SubmittedAt = now.AddDays(-15),
                        ProcessedAt = now.AddDays(-10),
                        ProcessedById = adminId,
                        CreatedAt = now.AddDays(-15),
                        CreatedById = tourGuideUserIds[0],
                        UpdatedAt = now.AddDays(-10),
                        UpdatedById = adminId,
                        IsActive = true,
                        IsDeleted = false
                    },
                    // TourGuide 2 - Adventure Specialist (Approved)
                    new TourGuideApplication
                    {
                        Id = Guid.Parse("e2f3a4b5-c6d7-8901-efa2-345678901bcd"),
                        UserId = tourGuideUserIds[1],
                        FullName = "Tran Thi Mai",
                        PhoneNumber = "0987654322",
                        Email = "tourguide2@example.com",
                        Experience = "4 years leading adventure tours in Ba Den Mountain and Dau Tieng Lake. Certified in mountain climbing and water sports.",
                        Languages = "English", // Legacy field
                        Skills = "Adventure", // ONLY Adventure skill
                        CurriculumVitae = "http://localhost:5267/uploads/cv/2024/12/tourguide2/cv.pdf",
                        CvOriginalFileName = "TranThiMai_CV.pdf",
                        CvFileSize = 1800000, // 1.8MB
                        CvContentType = "application/pdf",
                        CvFilePath = "uploads/cv/2024/12/tourguide2/cv.pdf",
                        Status = TourGuideApplicationStatus.Approved,
                        SubmittedAt = now.AddDays(-12),
                        ProcessedAt = now.AddDays(-8),
                        ProcessedById = adminId,
                        CreatedAt = now.AddDays(-12),
                        CreatedById = tourGuideUserIds[1],
                        UpdatedAt = now.AddDays(-8),
                        UpdatedById = adminId,
                        IsActive = true,
                        IsDeleted = false
                    },
                    // TourGuide 3 - Photography Specialist (Approved)
                    new TourGuideApplication
                    {
                        Id = Guid.Parse("f3a4b5c6-d7e8-9012-fab3-456789012cde"),
                        UserId = tourGuideUserIds[2],
                        FullName = "Le Minh Quan",
                        PhoneNumber = "0987654323",
                        Email = "tourguide3@example.com",
                        Experience = "3 years specializing in photography workshops and scenic tours. Expert in capturing beautiful moments and landscapes.",
                        Languages = "Japanese", // Legacy field
                        Skills = "Photography", // ONLY Photography skill
                        CurriculumVitae = "http://localhost:5267/uploads/cv/2024/12/tourguide3/cv.pdf",
                        CvOriginalFileName = "LeMinhQuan_CV.pdf",
                        CvFileSize = 2200000, // 2.2MB
                        CvContentType = "application/pdf",
                        CvFilePath = "uploads/cv/2024/12/tourguide3/cv.pdf",
                        Status = TourGuideApplicationStatus.Approved,
                        SubmittedAt = now.AddDays(-8),
                        ProcessedAt = now.AddDays(-5),
                        ProcessedById = adminId,
                        CreatedAt = now.AddDays(-8),
                        CreatedById = tourGuideUserIds[2],
                        UpdatedAt = now.AddDays(-5),
                        UpdatedById = adminId,
                        IsActive = true,
                        IsDeleted = false
                    }
                };

                _context.TourGuideApplications.AddRange(applications);
                await _context.SaveChangesAsync();
            }

            // Seed TourGuides operational records from approved applications
            if (!await _context.TourGuides.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var adminId = Guid.Parse("496eaa57-88aa-41bd-8abf-2aefa6cc47de");

                // Get all approved applications that don't have TourGuide records yet
                var approvedApplications = await _context.TourGuideApplications
                    .Where(a => a.Status == TourGuideApplicationStatus.Approved && a.IsActive)
                    .Include(a => a.User)
                    .ToListAsync();

                if (!approvedApplications.Any())
                {
                    // No approved applications to create TourGuides from
                    return;
                }

                var tourGuides = new List<TourGuide>();

                // Create TourGuide records from approved applications
                foreach (var application in approvedApplications)
                {
                    var tourGuide = new TourGuide
                    {
                        Id = Guid.NewGuid(),
                        UserId = application.UserId,
                        ApplicationId = application.Id,
                        FullName = application.FullName,
                        PhoneNumber = application.PhoneNumber,
                        Email = application.Email,
                        Experience = application.Experience,
                        Skills = application.Skills, // Copy skills from application
                        IsAvailable = true,
                        Rating = 0.00m, // Default rating for new guides
                        TotalToursGuided = 0, // Default tours guided
                        ApprovedAt = application.ProcessedAt ?? now,
                        ApprovedById = application.ProcessedById ?? adminId,
                        CreatedAt = now,
                        CreatedById = adminId,
                        IsActive = true,
                        IsDeleted = false
                    };

                    tourGuides.Add(tourGuide);
                }

                _context.TourGuides.AddRange(tourGuides);
                await _context.SaveChangesAsync();
            }

            // Seed 3 SpecialtyShops with different specialties
            if (!await _context.SpecialtyShops.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var adminId = Guid.Parse("496eaa57-88aa-41bd-8abf-2aefa6cc47de");

                // Ensure users exist before creating SpecialtyShops
                var existingSpecialtyShopUsers = await _context.Users
                    .Where(u => specialtyShopUserIds.Contains(u.Id))
                    .ToListAsync();

                if (existingSpecialtyShopUsers.Count < 3)
                {
                    // Skip creating SpecialtyShops if users don't exist
                    return;
                }

                var specialtyShops = new List<SpecialtyShop>
                {
                    // SpecialtyShop 1 - Traditional Handicrafts
                    new SpecialtyShop
                    {
                        Id = Guid.Parse("d4e5f6a7-b8c9-0123-def4-456789012345"),
                        UserId = specialtyShopUserIds[0],
                        ShopName = "Tay Ninh Traditional Handicrafts",
                        RepresentativeName = "Pham Van Thanh",
                        PhoneNumber = "0987654331",
                        Email = "specialtyshop1@example.com",
                        Address = "123 Cao Dai Street, Tay Ninh City, Tay Ninh Province",
                        Location = "123 Cao Dai Street, Tay Ninh City, Tay Ninh Province",
                        Description = "Authentic traditional handicrafts made by local artisans. Specializing in bamboo products, pottery, and traditional textiles.",
                        ShopType = "Handicrafts",
                        BusinessLicense = "TN-BL-2024-001",
                        OpeningHours = "08:00",
                        ClosingHours = "18:00",
                        IsShopActive = true,
                        Rating = 4.7m,
                        Wallet = 2000000, // Thêm số dư ví mẫu
                        CreatedAt = now.AddDays(-25),
                        CreatedById = specialtyShopUserIds[0],
                        UpdatedAt = now.AddDays(-20),
                        UpdatedById = adminId
                    },
                    // SpecialtyShop 2 - Local Food & Beverages
                    new SpecialtyShop
                    {
                        Id = Guid.Parse("e5f6a7b8-c9d0-1234-efa5-567890123456"),
                        UserId = specialtyShopUserIds[1],
                        ShopName = "Tay Ninh Delicious Foods",
                        RepresentativeName = "Nguyen Thi Lan",
                        PhoneNumber = "0987654332",
                        Email = "specialtyshop2@example.com",
                        Address = "456 Market Street, Tay Ninh City, Tay Ninh Province",
                        Location = "456 Market Street, Tay Ninh City, Tay Ninh Province",
                        Description = "Authentic Tay Ninh local foods and beverages. Famous for rice paper, dried fruits, and traditional sweets.",
                        ShopType = "Food",
                        BusinessLicense = "TN-BL-2024-002",
                        OpeningHours = "07:00",
                        ClosingHours = "19:00",
                        IsShopActive = true,
                        Rating = 4.5m,
                        Wallet = 3500000, // Thêm số dư ví mẫu
                        CreatedAt = now.AddDays(-18),
                        CreatedById = specialtyShopUserIds[1],
                        UpdatedAt = now.AddDays(-15),
                        UpdatedById = adminId
                    },
                    // SpecialtyShop 3 - Religious & Cultural Items
                    new SpecialtyShop
                    {
                        Id = Guid.Parse("f6a7b8c9-d0e1-2345-fab6-678901234567"),
                        UserId = specialtyShopUserIds[2],
                        ShopName = "Cao Dai Religious Items",
                        RepresentativeName = "Tran Van Minh",
                        PhoneNumber = "0987654333",
                        Email = "specialtyshop3@example.com",
                        Address = "789 Temple Road, Tay Ninh City, Tay Ninh Province",
                        Location = "789 Temple Road, Tay Ninh City, Tay Ninh Province",
                        Description = "Religious and cultural items related to Cao Dai faith and Vietnamese traditions. Books, incense, and ceremonial items.",
                        ShopType = "Religious",
                        BusinessLicense = "TN-BL-2024-003",
                        OpeningHours = "06:00",
                        ClosingHours = "20:00",
                        IsShopActive = true,
                        Rating = 4.8m,
                        Wallet = 500000, // Thêm số dư ví mẫu
                        CreatedAt = now.AddDays(-14),
                        CreatedById = specialtyShopUserIds[2],
                        UpdatedAt = now.AddDays(-12),
                        UpdatedById = adminId
                    }
                };

                _context.SpecialtyShops.AddRange(specialtyShops);
                await _context.SaveChangesAsync();
            }

            // Seed test products for the first specialty shop user
            if (!await _context.Products.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var firstSpecialtyShopUserId = specialtyShopUserIds[0]; // Shop 1 (Handicrafts)

                var testProducts = new List<Product>
                {
                    // Product 1 - Traditional Bamboo Basket
                    new Product
                    {
                        Id = Guid.Parse("aa111111-1111-1111-1111-111111111111"),
                        Name = "Giỏ tre truyền thống Tây Ninh",
                        Description = "Giỏ tre thủ công được làm từ tre già, đan theo kỹ thuật truyền thống của người dân Tây Ninh. Sản phẩm thân thiện với môi trường, bền đẹp, phù hợp để đựng đồ gia dụng hoặc làm quà tặng.",
                        Price = 150000m, // 150,000 VNĐ
                        QuantityInStock = 50,
                        Category = ProductCategory.Souvenir,
                        IsSale = false,
                        SalePercent = null,
                        SoldCount = 5,
                        ShopId = firstSpecialtyShopUserId,
                        CreatedAt = now.AddDays(-10),
                        CreatedById = firstSpecialtyShopUserId,
                        UpdatedAt = now.AddDays(-10),
                        UpdatedById = firstSpecialtyShopUserId,
                        IsActive = true,
                        IsDeleted = false
                    },
                    // Product 2 - Tay Ninh Pottery
                    new Product
                    {
                        Id = Guid.Parse("bb222222-2222-2222-2222-222222222222"),
                        Name = "Gốm sứ thủ công Tây Ninh",
                        Description = "Bộ ấm chén gốm sứ được làm thủ công bởi nghệ nhân địa phương. Thiết kế tinh xảo với họa tiết truyền thống, phù hợp để thưởng trà hoặc trang trí.",
                        Price = 280000m, // 280,000 VNĐ
                        QuantityInStock = 25,
                        Category = ProductCategory.Souvenir,
                        IsSale = true,
                        SalePercent = 10, // Giảm 10%
                        SoldCount = 8,
                        ShopId = firstSpecialtyShopUserId,
                        CreatedAt = now.AddDays(-8),
                        CreatedById = firstSpecialtyShopUserId,
                        UpdatedAt = now.AddDays(-3),
                        UpdatedById = firstSpecialtyShopUserId,
                        IsActive = true,
                        IsDeleted = false
                    },
                    // Product 3 - Traditional Textile
                    new Product
                    {
                        Id = Guid.Parse("cc333333-3333-3333-3333-333333333333"),
                        Name = "Thổ cẩm Tây Ninh",
                        Description = "Vải thổ cẩm dệt thủ công với họa tiết đặc trưng của đồng bào dân tộc Tây Ninh. Chất liệu bền, màu sắc tự nhiên từ thực vật. Có thể làm túi xách, trang trí nội thất.",
                        Price = 320000m, // 320,000 VNĐ
                        QuantityInStock = 15,
                        Category = ProductCategory.Clothing,
                        IsSale = false,
                        SalePercent = null,
                        SoldCount = 3,
                        ShopId = firstSpecialtyShopUserId,
                        CreatedAt = now.AddDays(-5),
                        CreatedById = firstSpecialtyShopUserId,
                        UpdatedAt = now.AddDays(-5),
                        UpdatedById = firstSpecialtyShopUserId,
                        IsActive = true,
                        IsDeleted = false
                    }
                };

                _context.Products.AddRange(testProducts);
                await _context.SaveChangesAsync();

                // Add product images for better testing
                var productImages = new List<ProductImage>
                {
                    // Images for Bamboo Basket
                    new ProductImage
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Url = "https://example.com/images/bamboo-basket-1.jpg",
                        CreatedAt = now.AddDays(-10),
                        CreatedById = firstSpecialtyShopUserId,
                        IsActive = true
                    },
                    // Images for Pottery   
                    new ProductImage
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        ProductId = Guid.Parse("222222-2222-2222-2222-222222222222"),
                        Url = "https://example.com/images/pottery-set-1.jpg",
                        CreatedAt = now.AddDays(-8),
                        CreatedById = firstSpecialtyShopUserId,
                        IsActive = true
                    },
                    // Images for Traditional Textile
                    new ProductImage
                    {
                        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        ProductId = Guid.Parse("333333-3333-3333-3333-333333333333"),
                        Url = "https://example.com/images/traditional-textile-1.jpg",
                        CreatedAt = now.AddDays(-5),
                        CreatedById = firstSpecialtyShopUserId,
                        IsActive = true
                    }
                };

                _context.ProductImages.AddRange(productImages);
                await _context.SaveChangesAsync();
            }

            // Seed SupportedBanks (Vietnamese banks)
            // ĐÃ CHUYỂN SANG ENUM, KHÔNG SEED BẢNG NÀY NỮA
            // if (!await _context.SupportedBanks.AnyAsync())
            // {
            //     var banks = new List<SupportedBank>
            //     {
            //         new SupportedBank { Name = "Vietcombank", Code = "VCB" },
            //         new SupportedBank { Name = "VietinBank", Code = "CTG" },
            //         new SupportedBank { Name = "BIDV", Code = "BIDV" },
            //         new SupportedBank { Name = "Techcombank", Code = "TCB" },
            //         new SupportedBank { Name = "Sacombank", Code = "STB" },
            //         new SupportedBank { Name = "ACB", Code = "ACB" },
            //         new SupportedBank { Name = "MB Bank", Code = "MB" },
            //         new SupportedBank { Name = "TPBank", Code = "TPB" },
            //         new SupportedBank { Name = "VPBank", Code = "VPB" },
            //         new SupportedBank { Name = "SHB", Code = "SHB" },
            //         new SupportedBank { Name = "HDBank", Code = "HDB" },
            //         new SupportedBank { Name = "VIB", Code = "VIB" },
            //         new SupportedBank { Name = "Eximbank", Code = "EIB" },
            //         new SupportedBank { Name = "SeABank", Code = "SEAB" },
            //         new SupportedBank { Name = "OCB", Code = "OCB" },
            //         new SupportedBank { Name = "MSB", Code = "MSB" },
            //         new SupportedBank { Name = "SCB", Code = "SCB" },
            //         new SupportedBank { Name = "DongA Bank", Code = "DAB" },
            //         new SupportedBank { Name = "LienVietPostBank", Code = "LPB" },
            //         new SupportedBank { Name = "ABBANK", Code = "ABB" },
            //         new SupportedBank { Name = "PVcomBank", Code = "PVC" },
            //         new SupportedBank { Name = "Nam A Bank", Code = "NAB" },
            //         new SupportedBank { Name = "Bac A Bank", Code = "BAB" },
            //         new SupportedBank { Name = "Saigonbank", Code = "SGB" },
            //         new SupportedBank { Name = "VietBank", Code = "VBB" },
            //         new SupportedBank { Name = "Kienlongbank", Code = "KLB" },
            //         new SupportedBank { Name = "PG Bank", Code = "PGB" },
            //         new SupportedBank { Name = "OceanBank", Code = "OJB" },
            //         new SupportedBank { Name = "Co-opBank", Code = "COOP" },
            //     };
            //     _context.SupportedBanks.AddRange(banks);
            //     await _context.SaveChangesAsync();
            // }
        }
    }
}