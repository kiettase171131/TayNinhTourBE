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
        }
    }
}