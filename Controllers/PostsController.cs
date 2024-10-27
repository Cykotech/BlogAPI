using BlogAPI.Controllers;
using BlogAPI.Data;
using BlogAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PostsController(BlogContext context) : ControllerBase
    {
        private readonly BlogContext _context = context;
        
        [HttpGet]
        public async Task<ActionResult<List<Post>>> GetAll()
        {
            var posts = await context.Posts.ToListAsync();

            if (posts == null)
            {
                return NotFound();
            }

            foreach (var post in posts)
            {
                var comments = await context.Comments.Where(c => c.PostId == post.Id).ToListAsync();

                post.Comments = comments;
            }

            return Ok(posts);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Post>> GetById(int id)
        {
            var singlePost = await context.Posts.FindAsync(id);

            if (singlePost == null)
            {
                return NotFound();
            }

            var comments = await context
                .Comments.Where(comment => comment.PostId == id)
                .ToListAsync();

            singlePost.Comments = comments;

            return Ok(singlePost);
        }

        [HttpPost]
        // [Authorize]
        public async Task<ActionResult<Post>> CreateNewPost(Post newPost)
        {
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim: {claim}");
            }
            var userClaim = await User.Claims.FirstOrDefaultAsync(u => u.Type == "sub");
            
            Console.WriteLine($"User stuffs: {userClaim}");
            
            var author = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userClaim);

            if (author == null)
            {
                return BadRequest();
            }
            
            newPost.Author = author;
            
            context.Posts.Add(newPost);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newPost.Id }, newPost);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Post>> UpdatePost(int id, Post post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }

            context.Entry(post).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Post>> DeletePost(int id)
        {
            var postToRemove = await context.Posts.FindAsync(id);

            if (postToRemove == null)
            {
                return NotFound();
            }

            context.Posts.Remove(postToRemove);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(int id)
        {
            return context.Posts.Any(post => post.Id == id);
        }
    }
}