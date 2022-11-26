using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Queries;

public class GetDogByIdQuery : IRequest<GetDogByIdQueryResult>
{
    public int DogId { get; init; }
}

public class GetDogByIdQueryResult
{
    public Dog Dog { get; init; }
}

internal class GetDogByIdQueryHandler : IRequestHandler<GetDogByIdQuery, GetDogByIdQueryResult>
{
    private readonly DogesDbContext _dbContext;


    public GetDogByIdQueryHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<GetDogByIdQueryResult> Handle(GetDogByIdQuery request, CancellationToken cancellationToken)
    {
        var dog = await _dbContext.Doges
            .Include(d => d.Photos)
            .FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);

        return new GetDogByIdQueryResult
        {
            Dog = dog
        };
    }
}