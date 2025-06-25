using Microsoft.EntityFrameworkCore;
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
                     new User
                     {
                        Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-345678901234"),
                        PasswordHash = "$2a$12$4UzizvZsV3N560sv3.VX9Otmjqx9VYCn7LzCxeZZm0s4N01/y92Ni", // 12345678h@
                        Email = "tourguide1@example.com",
                        PhoneNumber = "0987654323",
                        CreatedAt= DateTime.UtcNow,
                        UpdatedAt= DateTime.UtcNow,
                        IsDeleted = false,
                        IsVerified = true,
                        RoleId = Guid.Parse("e2f4a6b8-c1d3-4e5f-a7b9-c2d4e6f8a0b2"), // Tour Guide role
                        Name = "Tour Guide 1",
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

            // Seed TourTemplates for testing
            if (!await _context.TourTemplates.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var adminId = Guid.Parse("496eaa57-88aa-41bd-8abf-2aefa6cc47de");
                var tourCompanyId = Guid.Parse("7a5cbc0b-6082-4215-a90a-9c8cb1b7cc5c"); // Tour Company user ID

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
                        CreatedById = tourCompanyId,
                        UpdatedAt = now.AddDays(-5),
                        UpdatedById = tourCompanyId
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
                        CreatedById = tourCompanyId,
                        UpdatedAt = now.AddDays(-3),
                        UpdatedById = tourCompanyId
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
                        CreatedById = tourCompanyId,
                        UpdatedAt = now.AddDays(-1),
                        UpdatedById = tourCompanyId
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
                        CreatedById = tourCompanyId,
                        UpdatedAt = now.AddDays(-2),
                        UpdatedById = tourCompanyId
                    }
                };

                await _context.TourTemplates.AddRangeAsync(tourTemplates);
                await _context.SaveChangesAsync();
            }

            // Seed Tour Guide Applications for testing
            if (!await _context.TourGuideApplications.AnyAsync())
            {
                var now = DateTime.UtcNow;
                var testUser1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var testUser2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
                var adminId = Guid.Parse("496eaa57-88aa-41bd-8abf-2aefa6cc47de");

                var applications = new List<TourGuideApplication>
                {
                    // Application 1 - Pending (for testing file upload)
                    new TourGuideApplication
                    {
                        Id = Guid.Parse("d1e2f3a4-b5c6-7890-def1-234567890abc"),
                        UserId = testUser1Id,
                        FullName = "Test User 1",
                        PhoneNumber = "0987654321",
                        Email = "testuser1@example.com",
                        Experience = "2",
                        Languages = "Vietnamese, English",
                        CurriculumVitae = null, // No CV uploaded yet
                        CvOriginalFileName = null,
                        CvFileSize = null,
                        CvContentType = null,
                        CvFilePath = null,
                        Status = TourGuideApplicationStatus.Pending,
                        SubmittedAt = now.AddDays(-2),
                        CreatedAt = now.AddDays(-2),
                        CreatedById = testUser1Id,
                        IsActive = true,
                        IsDeleted = false
                    },
                    // Application 2 - With CV file (for testing download)
                    new TourGuideApplication
                    {
                        Id = Guid.Parse("e2f3a4b5-c6d7-8901-efa2-345678901bcd"),
                        UserId = testUser2Id,
                        FullName = "Test User 2",
                        PhoneNumber = "0987654322",
                        Email = "testuser2@example.com",
                        Experience = "3",
                        Languages = "Vietnamese, English, French",
                        CurriculumVitae = "http://localhost:5267/uploads/cv/2024/12/b2c3d4e5-f6a7-8901-bcde-f23456789012/sample_cv.pdf",
                        CvOriginalFileName = "TestUser2_CV.pdf",
                        CvFileSize = 1024000, // 1MB
                        CvContentType = "application/pdf",
                        CvFilePath = "uploads/cv/2024/12/b2c3d4e5-f6a7-8901-bcde-f23456789012/sample_cv.pdf",
                        Status = TourGuideApplicationStatus.Pending,
                        SubmittedAt = now.AddDays(-1),
                        CreatedAt = now.AddDays(-1),
                        CreatedById = testUser2Id,
                        IsActive = true,
                        IsDeleted = false
                    },
                    // Application 3 - Approved
                    new TourGuideApplication
                    {
                        Id = Guid.Parse("f3a4b5c6-d7e8-9012-fab3-456789012cde"),
                        UserId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-345678901234"), // Tour Guide user
                        FullName = "Tour Guide 1",
                        PhoneNumber = "0987654323",
                        Email = "tourguide1@example.com",
                        Experience = "5",
                        Languages = "Vietnamese, English, Japanese",
                        CurriculumVitae = "http://localhost:5267/uploads/cv/2024/12/c3d4e5f6-a7b8-9012-cdef-345678901234/approved_cv.pdf",
                        CvOriginalFileName = "TourGuide1_CV.pdf",
                        CvFileSize = 2048000, // 2MB
                        CvContentType = "application/pdf",
                        CvFilePath = "uploads/cv/2024/12/c3d4e5f6-a7b8-9012-cdef-345678901234/approved_cv.pdf",
                        Status = TourGuideApplicationStatus.Approved,
                        SubmittedAt = now.AddDays(-10),
                        ProcessedAt = now.AddDays(-8),
                        ProcessedById = adminId,
                        CreatedAt = now.AddDays(-10),
                        CreatedById = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-345678901234"),
                        UpdatedAt = now.AddDays(-8),
                        UpdatedById = adminId,
                        IsActive = true,
                        IsDeleted = false
                    }
                };

                _context.TourGuideApplications.AddRange(applications);
                await _context.SaveChangesAsync();
            }
        }
    }
}