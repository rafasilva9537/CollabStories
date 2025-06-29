namespace api.Dtos.HttpResponses;

public record MessageResponse
{
    public required string Message { get; init; }
}