namespace Access.API.DTO;

public sealed record EvaluateRequestDto(Guid UserId, string Resource, string Method, string? Action, string? ContextJson);