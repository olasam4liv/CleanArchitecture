using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Web.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));
            
            // Enable API versioning operation filter
            o.OperationFilter<ApiVersionOperationFilter>();

            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            o.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

            o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });

            o.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
            });
        });

        return services;
    }
}

internal sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = $"Clean Architecture API {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated ? "This API version has been deprecated." : "Clean Architecture boilerplate API"
            });
        }
        
        // Filter endpoints to only include those for the specific version
        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            if (apiDesc.RelativePath is null)
            {
                return false;
            }

            // Extract version from the route path (e.g., /api/v1/... or /api/v2/...)
            string relativePath = apiDesc.RelativePath;
            
            // Check if the endpoint has a version in its route
            if (relativePath.StartsWith("api/v1/", StringComparison.OrdinalIgnoreCase))
            {
                return docName == "v1";
            }
            
            if (relativePath.StartsWith("api/v2/", StringComparison.OrdinalIgnoreCase))
            {
                return docName == "v2";
            }

            // For endpoints without explicit versioning, include in all versions
            return true;
        });
    }
}

internal sealed class ApiVersionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null)
        {
            return;
        }

        // Remove version parameter from operation since it's in the URL path
        IOpenApiParameter? versionParameter = operation.Parameters
            .FirstOrDefault(p => p?.Name?.Equals("version", StringComparison.OrdinalIgnoreCase) ?? false);
        
        if (versionParameter is not null)
        {
            operation.Parameters.Remove(versionParameter);
        }
    }
}

