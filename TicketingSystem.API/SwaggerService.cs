using Microsoft.OpenApi.Models;

namespace TicketingSystem.Infrastructure;

public static class SwaggerService
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            var schema = new OpenApiSecurityScheme
            {
                Description = "JWT Auth Bearer Schema",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            c.AddSecurityDefinition("Bearer", schema);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    schema,new []{"Bearer"}
                }
            };

            c.AddSecurityRequirement(securityRequirement);

        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }
}
