using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Contracts.Database;
using Domain.Database;

namespace Domain.Queries;

public class SearchGodQuery : IRequest<SearchGodQueryResult>
{
    public string SearchRequest { get; init; }
}

public class SearchGodQueryResult
{
    public ICollection<Dog> Dogs { get; init; }
}

internal class SearchGodQueryHandler : IRequestHandler<SearchGodQuery, SearchGodQueryResult>
{
    private readonly DogesDbContext _dbContext;


    public SearchGodQueryHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<SearchGodQueryResult> Handle(SearchGodQuery request, CancellationToken cancellationToken)
    {
        var keyWords = request.SearchRequest.Split(' ').ToList();

        var allDogs = await _dbContext.Doges
            .Include(d => d.Photos)
            .ToListAsync();
        var dogs = allDogs
            .Where(d => request.SearchRequest.Contains(d.Name, System.StringComparison.OrdinalIgnoreCase) ||
                request.SearchRequest.Contains(d.Breed, System.StringComparison.OrdinalIgnoreCase) ||
                request.SearchRequest.Contains(d.Size, System.StringComparison.OrdinalIgnoreCase) ||
                d.About.Split(' ').Any(a => request.SearchRequest.Contains(a, System.StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return new SearchGodQueryResult
        {
            Dogs = dogs
        };
    }
}