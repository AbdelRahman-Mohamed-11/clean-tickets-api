using Ardalis.Result;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.Users.GetCurrentUser;
using TicketingSystem.Application.Users.GetUserById;
using TicketingSystem.Application.Users.List;
using TicketingSystem.Application.Users.Login;
using TicketingSystem.Application.Users.Register;
using TicketingSystem.Core.Dtos.Identity;
using TicketingSystem.Core.Dtos.Login;
using TicketingSystem.Core.Dtos.Register;

namespace TicketingSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IMediator mediator,
        ILogger<AuthController> logger,
        IValidator<RegisterUserCommand> registerValidator,
        IValidator<LoginUserCommand> loginValidator) : ControllerBase
    {

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            logger.LogInformation("Registration attempt for {UserName} / {Email}", dto.UserName, dto.Email);

            var cmd = new RegisterUserCommand(dto.UserName, dto.Email, dto.Password);
            var validation = await registerValidator.ValidateAsync(cmd);
            if (!validation.IsValid)
            {
                logger.LogWarning("Validation failed for registration: {Errors}",
                    string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

                foreach (var err in validation.Errors)
                    ModelState.AddModelError(err.PropertyName, err.ErrorMessage);

                return ValidationProblem(ModelState);
            }

            var result = await mediator.Send(cmd);
            if (result.Status == ResultStatus.Error)
            {
                logger.LogWarning("Registration failed for {Email}: {Errors}",
                    dto.Email, string.Join("; ", result.Errors));

                return BadRequest(result.Errors);
            }

            logger.LogInformation("User registered successfully: {UserId}", result.Value);
            // no GET endpoint to point at, so simply echo the location
            return Created($"/api/auth/{result.Value}", new { id = result.Value });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            logger.LogInformation("Login attempt for {Email}", dto.Email);

            var cmd = new LoginUserCommand(dto.Email, dto.Password);
            var validation = await loginValidator.ValidateAsync(cmd);
            if (!validation.IsValid)
            {
                logger.LogWarning("Validation failed for login: {Errors}",
                    string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

                foreach (var err in validation.Errors)
                    ModelState.AddModelError(err.PropertyName, err.ErrorMessage);

                return ValidationProblem(ModelState);
            }

            var result = await mediator.Send(cmd);
            if (result.Status == ResultStatus.Unauthorized)
            {
                logger.LogWarning("Login unauthorized for {Email}", dto.Email);
                return Unauthorized(result.Errors);
            }
            if (result.Status == ResultStatus.Error)
            {
                logger.LogWarning("Login failed for {Email}: {Errors}",
                    dto.Email, string.Join("; ", result.Errors));
                return BadRequest(result.Errors);
            }

            logger.LogInformation("User logged in successfully: {Email}", dto.Email);
            return Ok(result.Value);
        }

        [Authorize]
        [HttpGet("users")]
        [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]

        public async Task<IActionResult> GetAllUsers([FromQuery] UserRole user)
        {
            logger.LogInformation("Get All users");

            var users = await mediator.Send(new ListUsersQuery());

            logger.LogInformation("Get All users successfully");

            return Ok(users.Value);
        }

        [Authorize]
        [HttpGet("users/{Id:guid}")]
        [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var query = await mediator.Send(new GetUserByIdQuery(id));

            return query.Status == ResultStatus.NotFound
                ? NotFound(query.Errors)
                : Ok(query.Value);
        }

        [Authorize]

        [HttpGet("get-current-user")]
        [ProducesResponseType(typeof(GetUserDto), StatusCodes.Status200OK)]

        public async Task<IActionResult> GetCurrentUser()
        {
            var query = await mediator.Send(new GetCurrentUserQuery());

            return Ok(query.Value);
        }
    }
}
