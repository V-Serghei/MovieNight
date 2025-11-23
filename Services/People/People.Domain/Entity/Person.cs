namespace People.Domain.Entity;

public class Person
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string? KnownForDepartment { get; set; } 
    public DateTime? BirthDate { get; set; }
    public string? Country { get; set; }
    public string? Bio { get; set; }
    public Guid? ProfileImageId { get; set; } 
}