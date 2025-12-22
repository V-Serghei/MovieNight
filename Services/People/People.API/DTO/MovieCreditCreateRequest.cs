using People.Domain.Entity;

namespace People.API.DTO;

public record MovieCreditCreateRequest(
    Guid MovieId,
    Guid PersonId,
    CreditRole Role,
    string? CharacterName,
    int? Order
);