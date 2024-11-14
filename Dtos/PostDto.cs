namespace BlogAPI.Dtos;

public record PostDto(int Id, string Title, string Content, string? Author);