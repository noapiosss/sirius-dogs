using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Contracts.Database;
using Domain.Database;
using System;

namespace Domain.Queries;

public class SearchGodQuery : IRequest<SearchGodQueryResult>
{
    public string SearchRequest { get; init; }
    public int MaxAge { get; init; }
    public int Row { get; init; }
    public int Enclosure { get; init; }
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
        var searchRequest = string.IsNullOrEmpty(request.SearchRequest) ? "" : request.SearchRequest.ToLower();
        var minBirthDate = DateTime.UtcNow.AddMonths(-request.MaxAge);
        minBirthDate = minBirthDate.AddDays(-minBirthDate.Day);

        var dogs = await _dbContext.Doges
            .Where(d => d.BirthDate.Date >= minBirthDate.Date &&
                (request.Row == 0 || d.Row == request.Row) &&
                (request.Enclosure == 0 || d.Enclosure == request.Enclosure) &&                    
                (EF.Functions.Like(d.Name.ToLower(), $"%{searchRequest}%") ||
                EF.Functions.Like(d.Breed.ToLower(), $"%{searchRequest}%") ||
                EF.Functions.Like(d.Size.ToLower(), $"%{searchRequest}%") || 
                EF.Functions.Like(d.About.ToLower(), $"%{searchRequest}%")))
            .Include(d => d.Photos)
            .OrderByDescending(d => d.Id)
            .ToListAsync(cancellationToken);
        
        return new SearchGodQueryResult
        {
            Dogs = dogs
        };
    }
}