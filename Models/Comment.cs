using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Required] public string Content { get; set; }
        [Required] public int PostId { get; set; }
        public Post Post { get; set; }
        [Required] public int AuthorId { get; set; }
        public User Author { get; set; }
    }
}