using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Models
{
    public class Comment
    {
        public int Id { get; init; }
        [Required] public string Content { get; set; }
        [Required] public int PostId { get; init; }
        [Required] public Post Post { get; init; }
        [Required] public string AuthorId { get; init; }
        [Required] public BlogUser Author { get; init; }
    }
}