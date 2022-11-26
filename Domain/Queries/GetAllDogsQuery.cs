using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Queries;

public class GetAllDogsQuery : IRequest<GetAllDogsQueryResult>
{
}

public class GetAllDogsQueryResult
{
    public ICollection<Dog> Dogs { get; init; }
}

internal class GetAllDogsQueryHandler : IRequestHandler<GetAllDogsQuery, GetAllDogsQueryResult>
{
    private readonly DogesDbContext _dbContext;


    public GetAllDogsQueryHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<GetAllDogsQueryResult> Handle(GetAllDogsQuery request, CancellationToken cancellationToken)
    {
        var allDogs = await _dbContext.Doges.OrderByDescending(d => d.Id).ToListAsync(cancellationToken);

        return new GetAllDogsQueryResult
        {
            Dogs = allDogs
        };
    }
}