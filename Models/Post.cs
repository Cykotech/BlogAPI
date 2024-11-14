using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Models
{
    public class Post
    {
        public int Id { get; init; }
        [Required] public string Title { get; set; }
        [Required] public string Content { get; set; }
        [DataType(DataType.Date)] public DateTime CreatedDate { get; init; } = DateTime.Now;
        public List<Comment> Comments { get; init; } = [];
        public int Likes { get; init; } = 0;
        public string AuthorId { get; init; }
        public BlogUser Author { get; init; }
    }
}