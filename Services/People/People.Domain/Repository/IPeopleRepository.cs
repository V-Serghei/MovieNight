using People.Domain.Entity;

namespace People.Domain.Repository;

public interface IPeopleRepository
{
    // Person
    Task<Person> AddPersonAsync(Person person, CancellationToken ct);
    Task<Person?> GetPersonByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Person>> SearchPeopleByNameAsync(string name, int take, CancellationToken ct);
    Task<bool> PersonExistsAsync(Guid id, CancellationToken ct);

    // MovieCredit
    Task<MovieCredit> AddMovieCreditAsync(MovieCredit credit, CancellationToken ct);
    Task<IReadOnlyList<(MovieCredit Credit, Person Person)>> GetCreditsForMovieAsync(Guid movieId, CancellationToken ct);
}