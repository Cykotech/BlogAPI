using System.Security.Claims;
using BlogAPI.Dtos;
using BlogAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserManager<BlogUser> _userManager;
    private readonly SignInManager<BlogUser> _signInManager;

    public UsersController(UserManager<BlogUser> userManager, SignInManager<BlogUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        List<BlogUser> users = await _userManager.Users.ToListAsync();
        List<BlogUserDto> response = [];

        foreach (BlogUser user in users)
        {
            response.Add(new BlogUserDto(user.Id, user.UserName ?? "Anonymous"));
        }
        
        return Ok(response);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        string? currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
            return Unauthorized();
        
        BlogUser? user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null)
            return NotFound();
        
        BlogUserDto response = new(user.Id, user.UserName);
        
        return Ok(response);
    }

    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] object empty)
    {
        if (empty != null)
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
        
        return Unauthorized();
    }
}