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
        var keyWords = request.SearchRequest.Split(" ").ToList();

        var dogs = await _dbContext.Doges
            .Where(d => EF.Functions.Like(d.Name, $"%{request.SearchRequest}%") ||
                EF.Functions.Like(d.Breed, $"%{request.SearchRequest}%") ||
                EF.Functions.Like(d.Size, $"%{request.SearchRequest}%") || 
                EF.Functions.Like(d.About, $"%{request.SearchRequest}%"))
            .Include(d => d.Photos)
            .ToListAsync(cancellationToken);

        return new SearchGodQueryResult
        {
            Dogs = dogs
        };
    }
}