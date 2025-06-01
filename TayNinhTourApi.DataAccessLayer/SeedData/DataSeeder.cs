using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;

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
            await _context.Database.MigrateAsync();

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
                        Id = Guid.NewGuid(),
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
                        Id = Guid.NewGuid(),
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
            }

            await _context.SaveChangesAsync();
        }
    }
}
