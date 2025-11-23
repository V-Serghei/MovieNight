namespace People.API.DTO;

public record PersonCreateRequest(
    string FullName,
    string? KnownForDepartment,
    DateTime? BirthDate,
    string? Country,
    string? Bio,
    Guid? ProfileImageId
);