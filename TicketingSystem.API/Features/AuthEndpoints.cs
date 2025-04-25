using FluentValidation;
using MediatR;
using TicketingSystem.Application.Users.Login;
using TicketingSystem.Application.Users.Register;
using TicketingSystem.Core.Dtos.Login;
using TicketingSystem.Core.Dtos.Register;

namespace TicketingSystem.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/auth");

            group.MapPost("/register", async (
                RegisterDto dto,
                IValidator<RegisterUserCommand> validator,
                IMediator mediator) =>
            {
                var command = new RegisterUserCommand(dto.UserName, dto.Email, dto.Password);

                var validated = validator.Validate(command);

                if (!validated.IsValid)
                {
                    return Results.ValidationProblem(validated.ToDictionary());
                }

                var id = await mediator.Send(command);

                return Results.Created($"/api/auth/{id}", id);
            })
            .AllowAnonymous()
            .WithName("RegisterUser")
            .WithTags("Auth")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    
            group.MapPost("/login", async (
                LoginDto dto,
                IValidator<LoginUserCommand> validator,
                IMediator med) =>
            {
                var command = new LoginUserCommand(dto.Email, dto.Password);

                var validated = validator.Validate(command);

                if (!validated.IsValid)
                {
                    return Results.ValidationProblem(validated.ToDictionary());
                }

                var resp = await med.Send(command);
                
                return Results.Ok(resp);
            })
            .AllowAnonymous()
            .WithName("LoginUser")
            .WithTags("Auth")
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

            return group;
        }
    }
}
