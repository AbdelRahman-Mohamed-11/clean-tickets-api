using TicketingSystem.Api.Endpoints;
using TicketingSystem.API.Middlewares;
using TicketingSystem.Application;
using TicketingSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerDocumentation();

builder.Services.AddInfrastructure(builder.Configuration)
    .AddApplication(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


var app = builder.Build();

app.UseExceptionHandler();


if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapAuthEndpoints();


await SeedUsersWithRoles.InitializeAsync(app.Services);

app.Run();
