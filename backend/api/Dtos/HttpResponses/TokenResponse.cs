namespace api.Dtos.HttpResponses;

public record TokenResponse
{
    public required string Token { get; init; }
}