using System.Net.Mail;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Authentication.Clients;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Model.Responses;
using SharedKernel.Utilities;

namespace Infrastructure.Authentication;

internal sealed class AuthService(IApplicationDbContext context) : IAuthService
{
    public async Task<ResponseModel<CreateApiClientResponse>> RegisterClientAsync(ApiClientDto payload, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ResponseModel<CreateApiClientResponse>.Failure("Request cancelled", ResponseStatusCode.Failed.ResponseCode);
        }

        string email = payload.Email?.Trim() ?? string.Empty;
        string name = payload.Name?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
        {
            return ResponseModel<CreateApiClientResponse>.Failure("Invalid email format", ResponseStatusCode.ValidationError.ResponseCode);
        }

        if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
        {
            return ResponseModel<CreateApiClientResponse>.Failure("Name must be at least 3 characters long", ResponseStatusCode.ValidationError.ResponseCode);
        }

        bool clientExists = await context.ApiClients.AnyAsync(c => c.Email == email, cancellationToken);
        if (clientExists)
        {
            return ResponseModel<CreateApiClientResponse>.Failure("Client already exists", ResponseStatusCode.Conflict.ResponseCode);
        }

        string clientKey = await GenerateUniqueClientKeyAsync(cancellationToken);

        var newApiClient = new ApiClient
        {
            Iv = ServiceHelper.GenerateSecretCode(16),
            SecretKey = ServiceHelper.GenerateSecretCode(16),
            ClientKey = clientKey,
            Email = email,
            Name = name,
            IsActive = true
        };

        await context.ApiClients.AddAsync(newApiClient, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var response = new CreateApiClientResponse(newApiClient.Email, newApiClient.ClientKey, newApiClient.Name);

        return ResponseModel<CreateApiClientResponse>.Success(response, "Registration successful");
    }

    public async Task<ResponseModel<bool>> RevokeClientKeyAsync(string clientKey, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ResponseModel<bool>.Failure("Request cancelled", ResponseStatusCode.Failed.ResponseCode);
        }

        ApiClient? client = await context.ApiClients.FirstOrDefaultAsync(c => c.ClientKey == clientKey, cancellationToken);

        if (client is null)
        {
            return ResponseModel<bool>.Failure("Invalid client credentials", ResponseStatusCode.InvalidCredentials.ResponseCode);
        }

        if (!client.IsActive)
        {
            return ResponseModel<bool>.Success(true, "Client key already inactive");
        }

        client.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);

        return ResponseModel<bool>.Success(true, "Client key revoked successfully");
    }

    public async Task<ResponseModel<bool>> ValidateClientAsync(string clientKey, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ResponseModel<bool>.Failure("Request cancelled", ResponseStatusCode.Failed.ResponseCode);
        }

        bool clientExists = await context.ApiClients
            .AnyAsync(c => c.ClientKey == clientKey && c.IsActive, cancellationToken);

        return clientExists
            ? ResponseModel<bool>.Success(true, "Client validated successfully")
            : ResponseModel<bool>.Failure("Invalid client credentials", ResponseStatusCode.InvalidCredentials.ResponseCode);
    }

    public async Task<ResponseModel<GetApiClientResponse>> GetClientByKeyAsync(string clientKey, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ResponseModel<GetApiClientResponse>.Failure("Request cancelled", ResponseStatusCode.Failed.ResponseCode);
        }

        ApiClient? client = await context.ApiClients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClientKey == clientKey && c.IsActive, cancellationToken);

        if (client is null)
        {
            return ResponseModel<GetApiClientResponse>.Failure("Invalid client credentials", ResponseStatusCode.InvalidCredentials.ResponseCode);
        }

        var dto = new GetApiClientResponse(client.Email, client.ClientKey, client.Iv, client.SecretKey, client.IsActive);

        return ResponseModel<GetApiClientResponse>.Success(dto, "Client retrieved successfully");
    }

    private async Task<string> GenerateUniqueClientKeyAsync(CancellationToken cancellationToken)
    {
        string clientKey;
        do
        {
            clientKey = ServiceHelper.GenerateSecretCode(48);
        }
        while (await context.ApiClients.AnyAsync(c => c.ClientKey == clientKey, cancellationToken));

        return clientKey;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
