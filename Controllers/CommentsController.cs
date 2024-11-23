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
    public class CommentsController : ControllerBase
    {
        private readonly BlogContext _context;
        private readonly UserManager<BlogUser> _userManager;
        private readonly SignInManager<BlogUser> _signInManager;

        public CommentsController(BlogContext context, UserManager<BlogUser> userManager, SignInManager<BlogUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Comment>> GetCommentById(int id)
        {
            Comment? comment = await _context.Comments.Include(comment => comment.Author)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound();

            CommentDto response = new(comment.Id, comment.Content, comment.Author.UserName);

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<List<Comment>>> GetCommentsByPostId([FromQuery] int postId)
        {
            List<Comment> comments = await _context.Comments.Where(c => c.PostId == postId)
                .Include(comment => comment.Author).ToListAsync();
            List<CommentDto> response = new(comments.Count);

            foreach (Comment comment in comments)
            {
                response.Add(new CommentDto(comment.Id, comment.Content, comment.Author.UserName));
            }

            return Ok(response);
        }

        [HttpGet("{author}")]
        public async Task<ActionResult<List<Comment>>> GetCommentsByAuthor(string author)
        {
            BlogUser? user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == author);

            if (user == null)
                return NotFound();

            List<Comment> comments = await _context.Comments.Where(c => c.Author == user)
                .Include(comment => comment.Author).ToListAsync();
            List<CommentDto> commentsResponse = [];

            foreach (Comment comment in comments)
            {
                CommentDto dto = new(comment.Id, comment.Content, comment.Author.UserName);

                commentsResponse.Add(dto);
            }

            return Ok(commentsResponse);
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<Comment>> CreateNewComment([FromBody] CommentDto newComment,
            [FromQuery] int postId)
        {
            Post? post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return NotFound("Post not found");

            BlogUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            Comment commentEntity = new()
            {
                Content = newComment.Content,
                PostId = post.Id,
                Author = currentUser,
                AuthorId = currentUser.Id
            };

            _context.Comments.Add(commentEntity);
            await _context.SaveChangesAsync();

            CommentDto responseDto = new(commentEntity.Id, commentEntity.Content, commentEntity.Author.UserName);

            return CreatedAtAction(nameof(GetCommentById), new { id = commentEntity.Id }, responseDto);
        }

        [HttpPut("{id:int}"), Authorize]
        public async Task<ActionResult<Comment>> UpdateComment(CommentDto comment, int id)
        {
            if (!CommentExists(id))
                return NotFound("Comment not found");

            Comment? commentEntity = await _context.Comments.FindAsync(id);

            BlogUser? currentUser = await _userManager.GetUserAsync(User);

            if (!UserIsAuthor(currentUser, commentEntity.AuthorId))
                return Unauthorized();

            commentEntity.Content = comment.Content;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
                    return NotFound();
            }

            return Ok();
        }

        [HttpDelete("{id:int}"), Authorize]
        public async Task<ActionResult<Comment>> DeleteComment(int id)
        {
            if (!CommentExists(id))
                return NotFound();

            Comment? commentToRemove = await _context.Comments.FindAsync(id);

            BlogUser? currentUser = await _userManager.GetUserAsync(User);

            if (!UserIsAuthor(currentUser, commentToRemove.AuthorId))
                return Unauthorized();

            _context.Comments.Remove(commentToRemove);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(c => c.Id == id);
        }

        private static bool UserIsAuthor(BlogUser? user, string commentId)
        {
            return user?.Id == commentId;
        }
    }
}