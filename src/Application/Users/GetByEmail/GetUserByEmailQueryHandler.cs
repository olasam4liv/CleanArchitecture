using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Helper;
using SharedKernel.Model.Responses;

namespace Application.Users.GetByEmail;

internal sealed class GetUserByEmailQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetUserByEmailQuery, UserResponse>
{
    public async Task<ResponseModel<UserResponse>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        UserResponse? user = await context.Users
            .Where(u => u.Email == query.Email)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                FirstName = u.FirstName ?? string.Empty,
                LastName = u.LastName ?? string.Empty,
                Email = u.Email ?? string.Empty
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return ResponseModel<UserResponse>.Failure(
                MessageReader.GetMessage(ResponseStatusCode.UserNotFound.Value, "en"),
                ResponseStatusCode.UserNotFound.ResponseCode);
        }

        if (user.Id != userContext.UserId)
        {
            return ResponseModel<UserResponse>.Failure(
                MessageReader.GetMessage(ResponseStatusCode.Unauthorized.Value, "en"),
                ResponseStatusCode.Unauthorized.ResponseCode);
        }

        return ResponseModel<UserResponse>.Success(
            user,
            MessageReader.GetMessage(ResponseStatusCode.Successful.Value, "en"));
    }
}
