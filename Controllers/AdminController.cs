using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("urls")]
    public async Task<IActionResult> GetAllUrls()
    {
        var urls = await _context.ShortUrls.ToListAsync();
        return Ok(urls);
    }

    [HttpDelete("url/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var url = await _context.ShortUrls.FindAsync(id);

        if (url == null)
        {
            return NotFound();
        }

        _context.ShortUrls.Remove(url);
        await _context.SaveChangesAsync();

        return Ok();
    }
}