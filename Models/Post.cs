using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Models
{
    public class Post
    {
        public int Id { get; set; }
        [Required] public string Title { get; set; }
        [Required] public string Content { get; set; }
        [DataType(DataType.Date)] public DateTime CreatedDate { get; set; } = DateTime.Now;
        public List<Comment> Comments { get; set; } = new List<Comment> { };
        public int Likes { get; set; } = 0;
        public int AuthorId { get; set; }
        public User Author { get; set; }
    }
}