using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Queries;

public class GetShelterDogsQuery : IRequest<GetShelterDogsQueryResult>
{
}

public class GetShelterDogsQueryResult
{
    public ICollection<Dog> Dogs { get; init; }
}

internal class GetShelterDogsQueryHandler : IRequestHandler<GetShelterDogsQuery, GetShelterDogsQueryResult>
{
    private readonly DogesDbContext _dbContext;


    public GetShelterDogsQueryHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<GetShelterDogsQueryResult> Handle(GetShelterDogsQuery request, CancellationToken cancellationToken)
    {
        var allDogs = await _dbContext.Doges
            .Where(d => !d.WentHome)
            .OrderByDescending(d => d.Id)
            .ToListAsync(cancellationToken);

        return new GetShelterDogsQueryResult
        {
            Dogs = allDogs
        };
    }
}