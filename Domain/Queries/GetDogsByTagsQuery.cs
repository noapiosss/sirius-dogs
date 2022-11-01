using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Contracts.Database;
using Domain.Database;

namespace Domain.Queries;

public class GetDogsByTagsQuery : IRequest<GetDogsByTagsQueryResult>
{
    public ICollection<string> Tags { get; init; }
}

public class GetDogsByTagsQueryResult
{
    public ICollection<Dog> Dogs { get; init; }
}

internal class GetDogsByTagsQueryHandler : IRequestHandler<GetDogsByTagsQuery, GetDogsByTagsQueryResult>
{
    private readonly DogesDbContext _dbContext;


    public GetDogsByTagsQueryHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<GetDogsByTagsQueryResult> Handle(GetDogsByTagsQuery request, CancellationToken cancellationToken)
    {
        var dogsWithTags = await _dbContext.Doges
            .Include(d => d.Tags)
            .Where(d => d.Tags.Select(t => t.TagName).Intersect(request.Tags).Count() == request.Tags.Count())
            .ToListAsync(cancellationToken);

        return new GetDogsByTagsQueryResult
        {
            Dogs = dogsWithTags
        };
    }
}