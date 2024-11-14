using BlogAPI.Dtos;
using BlogAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserManager<BlogUser> _userManager;

    public UsersController(UserManager<BlogUser> userManager)
    {
        _userManager = userManager;
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
}