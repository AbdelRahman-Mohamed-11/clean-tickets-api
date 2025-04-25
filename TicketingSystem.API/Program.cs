using TicketingSystem.API.Middlewares;
using TicketingSystem.Application;
using TicketingSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerDocumentation();

builder.Services.AddInfrastructure(builder.Configuration)
    .AddApplication(builder.Configuration);

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

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

app.MapControllers();

await SeedUsersWithRoles.InitializeAsync(app.Services);

app.Run();
