using Microsoft.AspNetCore.Identity;

namespace BlogAPI.Models
{
    public class BlogUser : IdentityUser
    {
        public List<Post> Posts { get; set; } = [];
        public List<Comment> Comments { get; set; } = [];
    }
}