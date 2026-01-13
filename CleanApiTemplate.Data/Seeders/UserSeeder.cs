using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Core.Interfaces;
using CleanApiTemplate.Data.Persistence;
using CleanApiTemplate.Data.Seeders.Constants;
using Microsoft.EntityFrameworkCore;

namespace CleanApiTemplate.Data.Seeders;

public class UserSeeder(ICryptographyService cryptographyService) : ISeeder<ApplicationDbContext>
{
    private readonly ICryptographyService _cryptographyService = cryptographyService;

    public async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        if (!await context.Users.AnyAsync(cancellationToken) && await context.Roles.AnyAsync(cancellationToken: cancellationToken))
        {
            Role adminRole = await context.Roles.FirstAsync(r => r.Name == UserRolesList.Admin, cancellationToken);

            var users = new List<User>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Username = "Admin",
                    Email = "admin@example.com",
                    PasswordHash = _cryptographyService.HashPassword("P@ssw0rd"),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    IsDeleted = false,
                    UserRoles =
                    [
                        new UserRole
                        {
                            RoleId = adminRole.Id
                        }
                    ]
                }
            };

            await context.Users.AddRangeAsync(users, cancellationToken);
            await context.UserRoles.AddRangeAsync(users.SelectMany(u => u.UserRoles), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
