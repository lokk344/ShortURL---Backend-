using System.Security.Claims;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UrlController : ControllerBase
{
    private readonly AppDbContext _context;

    public UrlController(AppDbContext context)
    {
        _context = context;
    }
    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateUrlDto dto)
    {
        if (!Uri.IsWellFormedUriString(dto.OriginalUrl, UriKind.Absolute))
        {
            return BadRequest("Invalid URL");
        }

        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var shortCode = Guid.NewGuid()
            .ToString()
            .Substring(0, 6);

        var url = new ShortUrl
        {
            OriginalUrl = dto.OriginalUrl,
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow,
            ExpireAt = DateTime.UtcNow.AddDays(30),
            UserId = userId,
            ClickCount = 0
        };
        _context.ShortUrls.Add(url);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            shortCode = shortCode,

            shortUrl = $"{baseUrl}/api/url/redirect/{shortCode}",

            expireAt = url.ExpireAt
        });
    }

    [HttpGet("myurls")]
    [Authorize]
    public async Task<IActionResult> MyUrls()
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var urls = await _context.ShortUrls
            .Where(x => x.UserId == userId)
            .ToListAsync();

        return Ok(urls);
    }

    [HttpGet("redirect/{code}")]
    public async Task<IActionResult> RedirectUrl(string code)
    {
        var url = await _context.ShortUrls
            .FirstOrDefaultAsync(x => x.ShortCode == code);

        if (url == null)
        {
            return NotFound();
        }

        if (url.ExpireAt < DateTime.UtcNow)
        {
            return BadRequest("Link expired");
        }

        url.ClickCount++;
        await _context.SaveChangesAsync();

        return Redirect(url.OriginalUrl);
    }
}