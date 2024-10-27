using BlogAPI.Data;
using BlogAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CommentsController(BlogContext context) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Comment>> GetCommentById(int id)
        {
            var comment = await context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        [HttpPost]
        public async Task<ActionResult<Comment>> CreateNewComment(Comment newComment)
        {
            var post = await context.Posts.FindAsync(newComment.PostId);
            if (post == null)
            {
                return NotFound("Post not found");
            }

            context.Comments.Add(newComment);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCommentById), new { id = newComment.Id }, newComment);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Comment>> UpdateComment(int id, Comment comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }

            context.Entry(comment).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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
        public async Task<ActionResult<Comment>> DeleteComment(int id)
        {
            var comment = await context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            context.Comments.Remove(comment);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return context.Comments.Any(c => c.Id == id);
        }
    }
}
