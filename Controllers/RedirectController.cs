using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
public class RedirectController : ControllerBase
{
    private readonly AppDbContext _context;

    public RedirectController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("/{code}")]
    public async Task<IActionResult> RedirectShort(string code)
    {
        var url = await _context.ShortUrls
            .FirstOrDefaultAsync(x => x.ShortCode == code);

        if (url == null)
            return NotFound();

        if (url.ExpireAt < DateTime.UtcNow)
            return BadRequest("Link expired");

        url.ClickCount++;
        await _context.SaveChangesAsync();

        return Redirect(url.OriginalUrl);
    }
}