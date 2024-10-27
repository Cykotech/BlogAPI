using BlogAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers;

public class UsersController(BlogContext context) : ControllerBase
{
    private readonly DbContext _context = context;
    
}