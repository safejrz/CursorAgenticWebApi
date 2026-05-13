using System.Security.Claims;
using CursorAgenticWebApi.Application.Auth;
using CursorAgenticWebApi.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursorAgenticWebApi.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserRegistrationService registration,
    UserLoginService login,
    UserProfileService profile) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserApiRequest body, CancellationToken cancellationToken)
    {
        var result = await registration.RegisterAsync(
            new RegisterUserRequest(body.Email, body.Password, body.DisplayName),
            cancellationToken).ConfigureAwait(false);

        return result switch
        {
            ServiceResult<RegisterUserResponse>.Success s => Created("/api/auth/me", s.Value),
            ServiceResult<RegisterUserResponse>.Failure f => MapFailure(f),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError),
        };
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginApiRequest body, CancellationToken cancellationToken)
    {
        var result = await login.LoginAsync(new LoginRequest(body.Email, body.Password), cancellationToken).ConfigureAwait(false);

        return result switch
        {
            ServiceResult<LoginResponse>.Success s => Ok(s.Value),
            ServiceResult<LoginResponse>.Failure f => f.Code == "invalid_credentials"
                ? UnauthorizedProblem(f.Message)
                : MapFailure(f),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError),
        };
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return UnauthorizedProblem("Missing or invalid user id in token.");

        var result = await profile.GetProfileAsync(userId.Value, cancellationToken).ConfigureAwait(false);

        return result switch
        {
            ServiceResult<UserProfileResponse>.Success s => Ok(s.Value),
            ServiceResult<UserProfileResponse>.Failure f => f.Code == "not_found"
                ? NotFound(ProblemDetailsFor(f.Message, StatusCodes.Status404NotFound))
                : MapFailure(f),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError),
        };
    }

    private Guid? GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private IActionResult MapFailure<T>(ServiceResult<T>.Failure f) =>
        f.Code switch
        {
            "conflict" => Conflict(ProblemDetailsFor(f.Message, StatusCodes.Status409Conflict)),
            "validation" => BadRequest(ProblemDetailsFor(f.Message, StatusCodes.Status400BadRequest)),
            _ => BadRequest(ProblemDetailsFor(f.Message, StatusCodes.Status400BadRequest)),
        };

    private static ObjectResult UnauthorizedProblem(string detail) =>
        new(ProblemDetailsFor(detail, StatusCodes.Status401Unauthorized))
        {
            StatusCode = StatusCodes.Status401Unauthorized,
        };

    private static ProblemDetails ProblemDetailsFor(string detail, int status) =>
        new()
        {
            Title = GetTitle(status),
            Detail = detail,
            Status = status,
        };

    private static string GetTitle(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "Error",
    };
}

public sealed record RegisterUserApiRequest(string Email, string Password, string DisplayName);

public sealed record LoginApiRequest(string Email, string Password);
