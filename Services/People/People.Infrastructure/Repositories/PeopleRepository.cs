using Microsoft.EntityFrameworkCore;
using People.Domain.Entity;
using People.Domain.Repository;
using People.Infrastructure.Data;

namespace People.Infrastructure.Repositories;

public class PeopleRepository : IPeopleRepository
{
    private readonly PeopleDbContext _db;

    public PeopleRepository(PeopleDbContext db)
    {
        _db = db;
    }

    public async Task<Person> AddPersonAsync(Person person, CancellationToken ct)
    {
        await _db.People.AddAsync(person, ct);
        await _db.SaveChangesAsync(ct);
        return person;
    }

    public async Task<Person?> GetPersonByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.People.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Person>> SearchPeopleByNameAsync(string name, int take, CancellationToken ct)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Array.Empty<Person>();

        return await _db.People
            .Where(p => EF.Functions.Like(p.FullName, $"%{name}%"))
            .OrderBy(p => p.FullName)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<bool> PersonExistsAsync(Guid id, CancellationToken ct)
    {
        return await _db.People.AnyAsync(p => p.Id == id, ct);
    }

    public async Task<MovieCredit> AddMovieCreditAsync(MovieCredit credit, CancellationToken ct)
    {
        if (credit.Order <= 0)
        {
            var maxOrder = await _db.MovieCredits
                .Where(c => c.MovieId == credit.MovieId && c.Role == credit.Role)
                .Select(c => (int?)c.Order)
                .MaxAsync(ct); 

            credit.Order = (maxOrder ?? 0) + 1;
        }

        await _db.MovieCredits.AddAsync(credit, ct);
        await _db.SaveChangesAsync(ct);
        return credit;
    }

    public async Task<IReadOnlyList<(MovieCredit Credit, Person Person)>> GetCreditsForMovieAsync(Guid movieId, CancellationToken ct)
    {
        var query =
            from c in _db.MovieCredits
            join p in _db.People on c.PersonId equals p.Id
            where c.MovieId == movieId
            orderby c.Order
            select new { c, p };

        var list = await query.ToListAsync(ct);

        return list
            .Select(x => (x.c, x.p))
            .ToList();
    }
    
}