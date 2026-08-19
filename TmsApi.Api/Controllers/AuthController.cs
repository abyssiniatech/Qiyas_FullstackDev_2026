using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Auth;
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
        // Check whether the user already exists
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            // Generic response prevents account enumeration
            return Ok(new
            {
                message = "Registration request received."
            });
        }

        // Create Identity user
        // We use the email as the username.
        var user = new TmsUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        // UserManager applies the configured password policy
        var result =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!result.Succeeded)
        {
            var errors =
                result.Errors.Select(e => e.Description);

            return BadRequest(new
            {
                errors
            });
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            var roleResult =
                await _roleManager.CreateAsync(
                    new IdentityRole(request.Role));

            if (!roleResult.Succeeded)
            {
                return BadRequest(new
                {
                    errors = roleResult.Errors.Select(e => e.Description)
                });
            }
        }

        // Assign role to user
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

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {


        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        // =====================================================
        // ACCOUNT LOCKOUT CHECK
        // =====================================================

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

        // =====================================================
        // VERIFY PASSWORD
        // =====================================================

        var validPassword =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!validPassword)
        {
            // Increment failed login counter.
            // Identity will lock the account when the configured
            // maximum failed attempts is reached.
            await _userManager.AccessFailedAsync(user);

            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        // =====================================================
        // SUCCESSFUL LOGIN
        // =====================================================

        await _userManager.ResetAccessFailedCountAsync(user);

        // Get user's roles
        var roles =
            await _userManager.GetRolesAsync(user);

        // Generate JWT access token
        var accessToken =
            _tokenService.GenerateJwt(
                user,
                roles);

        // =====================================================
        // CREATE INITIAL REFRESH TOKEN
        // =====================================================

        var refreshToken = new RefreshToken
        {
            Token =
                Guid.NewGuid().ToString("N"),

            UserId =
                user.Id,

            ExpiresAt =
                DateTime.UtcNow.AddDays(7),

            IsUsed = false,

            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        // =====================================================
        // RETURN TOKEN PAIR
        // =====================================================

        return Ok(new
        {
            accessToken,

            refreshToken =
                refreshToken.Token
        });
    }

    // =========================================================
    // REFRESH REQUEST
    // =========================================================

    public record RefreshRequest(
        string RefreshToken);

    // =========================================================
    // REFRESH TOKEN
    // POST: /api/auth/refresh
    // =========================================================

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request)
    {
        // =====================================================
        // FIND REFRESH TOKEN
        // =====================================================

        var storedToken =
            await _context.RefreshTokens
                .FirstOrDefaultAsync(
                    rt =>
                        rt.Token ==
                        request.RefreshToken);

        if (storedToken == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid refresh token."
            });
        }

        // =====================================================
        // THEFT DETECTION
        // =====================================================

        if (storedToken.IsUsed)
        {
            var userTokens =
                await _context.RefreshTokens
                    .Where(
                        rt =>
                            rt.UserId ==
                            storedToken.UserId)
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

        // =====================================================
        // CHECK REVOCATION / EXPIRATION
        // =====================================================

        if (storedToken.IsRevoked ||
            storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return Unauthorized(new
            {
                detail =
                    "Refresh token expired or revoked."
            });
        }

        // =====================================================
        // FIND USER
        // =====================================================

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

        // =====================================================
        // MARK OLD TOKEN AS USED
        // =====================================================

        storedToken.IsUsed = true;

        // =====================================================
        // GET USER ROLES
        // =====================================================

        var roles =
            await _userManager.GetRolesAsync(user);

        // =====================================================
        // GENERATE NEW ACCESS TOKEN
        // =====================================================

        var newAccessToken =
            _tokenService.GenerateJwt(
                user,
                roles);

        // =====================================================
        // CREATE NEW REFRESH TOKEN
        // =====================================================

        var newRefreshToken = new RefreshToken
        {
            Token =
                Guid.NewGuid().ToString("N"),

            UserId =
                storedToken.UserId,

            ExpiresAt =
                DateTime.UtcNow.AddDays(7),

            IsUsed = false,

            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);

        // =====================================================
        // SAVE ROTATION
        // =====================================================

        await _context.SaveChangesAsync();

        // =====================================================
        // RETURN NEW TOKEN PAIR
        // =====================================================

        return Ok(new
        {
            accessToken = newAccessToken,

            refreshToken =
                newRefreshToken.Token
        });
    }
}