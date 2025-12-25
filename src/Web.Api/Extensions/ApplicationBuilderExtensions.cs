using Asp.Versioning.ApiExplorer;

namespace Web.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    {
        IApiVersionDescriptionProvider apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (ApiVersionDescription description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });

        return app;
    }
}
