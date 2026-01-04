using Application.Abstractions.Authentication;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authentication;

internal sealed class IdentityService(UserManager<User> userManager, SignInManager<User> signInManager) : IIdentityService
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return userManager.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public Task<IdentityResult> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        return userManager.CreateAsync(user, password);
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<SignInResult> CheckPasswordSignInAsync(User user, string password, bool lockoutOnFailure)
    {
        return signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
    }
}
