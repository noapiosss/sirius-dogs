using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Contracts.Database;
using Domain.Database;

namespace Domain.Queries;

public class GetDogByIdQuery : IRequest<GetDogByIdQueryResult>
{
    public int DogId { get; init; }
}

public class GetDogByIdQueryResult
{
    public Dog Dog { get; init; }
    public ICollection<string> PhotosPath { get; init; } 
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
        var dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);
        var dogPhotosPath = await _dbContext.Images.Where(i => i.DogId == request.DogId).Select(i => i.PhotoPath).ToListAsync(cancellationToken);

        return new GetDogByIdQueryResult
        {
            Dog = dog,
            PhotosPath = dogPhotosPath
        };
    }
}