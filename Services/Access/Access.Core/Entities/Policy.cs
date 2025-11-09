namespace Access.Core.Entities;

public class Policy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Resource { get; set; } = default!;
    public string? Method { get; set; }
    public string? Action { get; set; }
    public string? ConditionJson { get; set; }
    public Access.Core.Enums.Effect Effect { get; set; } = Access.Core.Enums.Effect.Allow;
    public string? Description { get; set; }
}
