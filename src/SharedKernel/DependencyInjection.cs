using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Helper;
using SharedKernel.Helper.Interfaces;

namespace SharedKernel;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedKernel(this IServiceCollection services)
    {   
        services.AddSingleton<ISerializerService, SerializerService>();
        return services;
    }
}
