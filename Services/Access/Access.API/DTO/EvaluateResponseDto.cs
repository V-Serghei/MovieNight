namespace Access.API.DTO;

public sealed record EvaluateResponseDto(bool Allow, string Reason, string? MatchedPolicyId, string? MatchedDescription);