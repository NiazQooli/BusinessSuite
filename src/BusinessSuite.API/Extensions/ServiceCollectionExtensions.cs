using Asp.Versioning;

namespace BusinessSuite.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
            {
                Title = "BusinessSuite API",
                Version = "v1",
                Description = "BusinessSuite REST API built with Clean Architecture"
            });
        });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AngularClient", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:4200",
                        "https://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddHealthChecks();

        return services;
    }
}
