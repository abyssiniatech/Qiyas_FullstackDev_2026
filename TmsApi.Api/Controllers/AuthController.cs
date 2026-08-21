using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<TmsUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context,
        TokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _tokenService = tokenService;
    }

    // =========================================================
    // REGISTER
    // POST: /api/auth/register
    // =========================================================

    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string Role);

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return Ok(new
            {
                message = "Registration request received."
            });
        }

        var user = new TmsUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(e => e.Description)
            });
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            var roleResult = await _roleManager.CreateAsync(
                new IdentityRole(request.Role));

            if (!roleResult.Succeeded)
            {
                return BadRequest(new
                {
                    errors = roleResult.Errors
                        .Select(e => e.Description)
                });
            }
        }

        var roleAssignmentResult =
            await _userManager.AddToRoleAsync(
                user,
                request.Role);

        if (!roleAssignmentResult.Succeeded)
        {
            return BadRequest(new
            {
                errors = roleAssignmentResult.Errors
                    .Select(e => e.Description)
            });
        }

        return Ok(new
        {
            message = "Registration successful."
        });
    }

    // =========================================================
    // LOGIN
    // POST: /api/auth/login
    // =========================================================

    public record LoginRequest(
        string Email,
        string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user =
            await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                new
                {
                    detail =
                        "Account locked due to multiple failed login attempts."
                });
        }

        var validPassword =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);

            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles =
            await _userManager.GetRolesAsync(user);

        // -----------------------------------------------------
        // Generate short-lived access token
        // -----------------------------------------------------

        var accessToken =
            _tokenService.GenerateJwt(
                user,
                roles);

        // -----------------------------------------------------
        // Generate initial refresh token
        // -----------------------------------------------------

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token
        });
    }

    // =========================================================
    // REFRESH REQUEST
    // =========================================================

    public record RefreshRequest(
        string RefreshToken);

    // =========================================================
    // REFRESH TOKEN ROTATION
    // POST: /api/auth/refresh
    // =========================================================

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request)
    {
        // -----------------------------------------------------
        // 1. Validate request
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Unauthorized(new
            {
                detail = "Refresh token is required."
            });
        }

        // -----------------------------------------------------
        // 2. Find refresh token
        // -----------------------------------------------------

        var storedToken =
            await _context.RefreshTokens
                .SingleOrDefaultAsync(
                    rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid refresh token."
            });
        }

        // -----------------------------------------------------
        // 3. Theft detection
        //
        // An already-used refresh token must NEVER be accepted.
        // Reuse means the token may have been stolen.
        // Revoke every refresh token belonging to the user.
        // -----------------------------------------------------

        if (storedToken.IsUsed)
        {
            var userTokens =
                await _context.RefreshTokens
                    .Where(rt =>
                        rt.UserId == storedToken.UserId &&
                        !rt.IsRevoked)
                    .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            return Unauthorized(new
            {
                detail =
                    "Token theft detected. All user sessions revoked."
            });
        }

        // -----------------------------------------------------
        // 4. Check expiration/revocation
        // -----------------------------------------------------

        if (storedToken.IsRevoked)
        {
            return Unauthorized(new
            {
                detail = "Refresh token has been revoked."
            });
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return Unauthorized(new
            {
                detail = "Refresh token has expired."
            });
        }

        // -----------------------------------------------------
        // 5. Find user
        // -----------------------------------------------------

        var user =
            await _userManager.FindByIdAsync(
                storedToken.UserId);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "User no longer exists."
            });
        }

        // -----------------------------------------------------
        // 6. Get current roles
        // -----------------------------------------------------

        var roles =
            await _userManager.GetRolesAsync(user);

        // -----------------------------------------------------
        // 7. Generate NEW access token
        // -----------------------------------------------------

        var newAccessToken =
            _tokenService.GenerateJwt(
                user,
                roles);

        // -----------------------------------------------------
        // 8. Mark OLD refresh token as used
        // -----------------------------------------------------

        storedToken.IsUsed = true;

        // -----------------------------------------------------
        // 9. Generate NEW refresh token
        // -----------------------------------------------------

        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);

        // -----------------------------------------------------
        // 10. Persist rotation
        // -----------------------------------------------------

        await _context.SaveChangesAsync();

        // -----------------------------------------------------
        // 11. Return NEW token pair
        // -----------------------------------------------------

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token
        });
    }
}