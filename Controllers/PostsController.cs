using BlogAPI.Data;
using BlogAPI.Dtos;
using BlogAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly BlogContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public PostsController(BlogContext context, UserManager<BlogUser> userManager)
        {
            _context = context ?? throw new NullReferenceException();
            _userManager = userManager ?? throw new NullReferenceException();
        }

        [HttpGet]
        public async Task<ActionResult<List<PostDto>>> GetAll()
        {
            List<Post> posts = await _context.Posts.Include(post => post.Author).ToListAsync();
            List<PostDto> response = [];

            foreach (Post post in posts)
            {
                response.Add(new PostDto(post.Id, post.Title, post.Content, post.Author.UserName, post.CreatedDate, post.Likes));
            }

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PostDto>> GetById(int id)
        {
            Post? post = await _context.Posts
                .Include(post => post.Author)
                .FirstOrDefaultAsync(post => post.Id == id);

            if (post == null)
                return NotFound();

            PostDto response = new(post.Id, post.Title, post.Content, post.Author.UserName, post.CreatedDate, post.Likes);

            return Ok(response);
        }

        [HttpGet("{authorUserName}")]
        public async Task<ActionResult<List<PostDto>>> GetByAuthor(string authorUserName)
        {
            if (!_context.Users.Any(u => u.UserName == authorUserName))
                return NotFound("User not registered");

            List<Post> postsByAuthor = await _context.Posts.Include(post => post.Author)
                .Where(p => p.Author.UserName == authorUserName)
                .ToListAsync();
            List<PostDto> response = [];

            foreach (Post post in postsByAuthor)
            {
                response.Add(new PostDto(post.Id, post.Title, post.Content, post.Author.UserName, post.CreatedDate, post.Likes));
            }

            return Ok(response);
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<Post>> CreateNewPost([FromBody] PostDto newPost)
        {
            BlogUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            Post postEntity = new()
            {
                Title = newPost.Title,
                Content = newPost.Content,
                Author = currentUser,
                AuthorId = currentUser.Id
            };

            _context.Posts.Add(postEntity);
            await _context.SaveChangesAsync();

            PostDto responseDto = new(postEntity.Id, postEntity.Title, postEntity.Content, postEntity.Author.UserName!,
                postEntity.CreatedDate);

            return CreatedAtAction(nameof(GetById), new { id = postEntity.Id }, responseDto);
        }

        [Authorize, HttpPut("{id:int}")]
        public async Task<ActionResult<Post>> UpdatePost(int id, PostDto post)
        {
            if (!PostExists(id))
                return NotFound();

            Post? postEntity = await _context.Posts.Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            BlogUser? currentUser = await _userManager.GetUserAsync(User);

            if (!UserIsAuthor(currentUser, postEntity!.Author.Id))
                return Unauthorized();

            postEntity.Title = post.Title;
            postEntity.Content = post.Content;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id:int}"), Authorize]
        public async Task<ActionResult<Post>> DeletePost(int id)
        {
            if (!PostExists(id))
                return NotFound();

            Post? postToRemove = await _context.Posts.Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            BlogUser? currentUser = await _userManager.GetUserAsync(User);

            if (!UserIsAuthor(currentUser, postToRemove!.Author.Id))
                return Unauthorized();

            _context.Posts.Remove(postToRemove);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(post => post.Id == id);
        }

        private static bool UserIsAuthor(BlogUser? user, string authorId)
        {
            return user?.Id == authorId;
        }
    }
}