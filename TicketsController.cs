using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISP_Portal.API.Data;
using ISP_Portal.API.Models;
using ISP_Portal.API.Models.Dtos;
using ISP_Portal.API.Services;

namespace ISP_Portal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var emailTaken = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailTaken)
            return Conflict("an account with this email already exists");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Phone = dto.Phone,
            Address = dto.Address,
            Role = UserRole.Customer // public signup always creates a customer, staff accounts get added separately
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(BuildAuthResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("invalid email or password");

        return Ok(BuildAuthResponse(user));
    }

    private AuthResponseDto BuildAuthResponse(User user)
    {
        return new AuthResponseDto
        {
            Token = _tokenService.GenerateToken(user),
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}
