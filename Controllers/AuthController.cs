using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext context, JwtService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        //
        // NORMALIZE INPUT
        //
        var username = dto.Username.Trim().ToLower();
        var email = dto.Email.Trim().ToLower();

        //
        // CHECK IF EMAIL EXISTS
        //
        var existingEmail = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

        if (existingEmail != null)
        {
            return BadRequest("Email already exists");
        }

        //
        // CHECK IF USERNAME EXISTS
        //
        var existingUsername = await _context.Users
            .FirstOrDefaultAsync(x => x.Username.ToLower() == username);

        if (existingUsername != null)
        {
            return BadRequest("Username already exists");
        }

        //
        // HASH PASSWORD
        //
        string passwordHash =
            BCrypt.Net.BCrypt.HashPassword(dto.Password);

        //
        // CREATE USER
        //
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = "User"
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok("Register successful");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var username = dto.Username.Trim().ToLower();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Username == username);

        if (user == null)
        {
            return BadRequest("User not found");
        }

        bool valid = BCrypt.Net.BCrypt.Verify(
            dto.Password,
            user.PasswordHash
        );

        if (!valid)
        {
            return BadRequest("Wrong password");
        }

        var token = _jwt.Generate(user);

        return Ok(new
        {
            token,
            user.Username,
            user.Role
        });
    }
}