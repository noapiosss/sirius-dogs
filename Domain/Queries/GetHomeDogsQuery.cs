using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Contracts.Database;
using Domain.Database;

namespace Domain.Queries;

public class GetHomeDogsQuery : IRequest<GetHomeDogsQueryResult>
{
}

public class GetHomeDogsQueryResult
{
    public ICollection<Dog> Dogs { get; init; }
}

internal class GetHomeDogsQueryHandler : IRequestHandler<GetHomeDogsQuery, GetHomeDogsQueryResult>
{
    private readonly DogesDbContext _dbContext;


    public GetHomeDogsQueryHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<GetHomeDogsQueryResult> Handle(GetHomeDogsQuery request, CancellationToken cancellationToken)
    {
        var allDogs = await _dbContext.Doges
            .Where(d => d.WentHome)
            .OrderByDescending(d => d.Id)
            .ToListAsync(cancellationToken);

        return new GetHomeDogsQueryResult
        {
            Dogs = allDogs
        };
    }
}