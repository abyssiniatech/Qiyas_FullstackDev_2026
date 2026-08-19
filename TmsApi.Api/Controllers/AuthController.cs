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
    // LOGIN
    // POST: /api/auth/login
    // =========================================================

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user =
            await _userManager.FindByEmailAsync(request.Username);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid credentials."
            });
        }

        // Account lockout check
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

        // Verify password
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

        // Successful login
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
        // Issue initial refresh token
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

        // Return both tokens
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
        // Find refresh token
        var storedToken =
            await _context.RefreshTokens
                .FirstOrDefaultAsync(
                    rt =>
                        rt.Token ==
                        request.RefreshToken);

        // Token does not exist
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
        // If an already-used refresh token is submitted,
        // revoke every refresh token belonging to that user.
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
        // CHECK EXPIRATION / REVOCATION
        // =====================================================

        if (storedToken.IsRevoked ||
            storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(new
            {
                detail =
                    "Refresh token expired or revoked."
            });
        }

        // =====================================================
        // MARK OLD TOKEN AS USED
        // =====================================================

        storedToken.IsUsed = true;


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
        // GET ROLES
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
        // SAVE ROTATION
        // =====================================================

        await _context.SaveChangesAsync();


        // =====================================================
        // RETURN NEW TOKEN PAIR
        // =====================================================

        return Ok(new
        {
            accessToken =
                newAccessToken,

            refreshToken =
                newRefreshToken.Token
        });
    }
}