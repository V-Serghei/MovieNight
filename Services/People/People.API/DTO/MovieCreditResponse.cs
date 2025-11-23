using People.Domain.Entity;

namespace People.API.DTO;

public record MovieCreditResponse(
    Guid Id,
    Guid MovieId,
    Guid PersonId,
    CreditRole Role,
    string? CharacterName,
    int? Order,
    string PersonFullName,
    Guid? PersonProfileImageId 
);