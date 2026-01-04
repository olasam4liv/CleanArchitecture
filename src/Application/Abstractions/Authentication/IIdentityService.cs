using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<IdentityResult> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);

    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<SignInResult> CheckPasswordSignInAsync(User user, string password, bool lockoutOnFailure);
}
