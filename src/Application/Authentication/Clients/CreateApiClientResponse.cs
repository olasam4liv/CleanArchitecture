namespace Application.Authentication.Clients;

public sealed record CreateApiClientResponse(string Email, string ClientKey, string Name);
public sealed record GetApiClientResponse(string Email, string ClientKey, string ClientIv, string SecretKey, bool IsActive);
