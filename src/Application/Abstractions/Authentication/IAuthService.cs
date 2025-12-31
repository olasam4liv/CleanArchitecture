using Application.Authentication.Clients;
using SharedKernel.Model.Responses;

namespace Application.Abstractions.Authentication;

public interface IAuthService
{
    Task<ResponseModel<CreateApiClientResponse>> RegisterClientAsync(ApiClientDto payload, CancellationToken cancellationToken);

    Task<ResponseModel<bool>> RevokeClientKeyAsync(string clientKey, CancellationToken cancellationToken);

    Task<ResponseModel<bool>> ValidateClientAsync(string clientKey, CancellationToken cancellationToken);

    Task<ResponseModel<GetApiClientResponse>> GetClientByKeyAsync(string clientKey, CancellationToken cancellationToken);
}
