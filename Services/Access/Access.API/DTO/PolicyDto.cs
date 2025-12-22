namespace Access.API.DTO;

public sealed record PolicyDto(Guid Id, string Resource, string? Method, string? Action, string? ConditionJson, string? Description, string Effect);