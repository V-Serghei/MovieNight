namespace People.API.DTO;

public record PersonResponse(
    Guid Id,
    string FullName,
    string? KnownForDepartment,
    DateTime? BirthDate,
    string? Country,
    string? Bio,
    Guid? ProfileImageId
);
