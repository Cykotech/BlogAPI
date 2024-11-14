namespace BlogAPI.Dtos;

public record CommentDto(int Id, string Content, string? AuthorId);