using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Queries
{
    public class GetDogsByWentHomeQuery : IRequest<GetDogsByWentHomeQueryResult>
    {
        public bool WentHome { get; init; }
        public int DogsPerPage { get; init; }
        public int Page { get; init; }
    }

    public class GetDogsByWentHomeQueryResult
    {
        public ICollection<Dog> Dogs { get; init; }
        public int DogsCount { get; init; }
    }

    internal class GetDogsByWentHomeQueryHandler : IRequestHandler<GetDogsByWentHomeQuery, GetDogsByWentHomeQueryResult>
    {
        private readonly DogesDbContext _dbContext;


        public GetDogsByWentHomeQueryHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetDogsByWentHomeQueryResult> Handle(GetDogsByWentHomeQuery request, CancellationToken cancellationToken)
        {
            int dogsCount = await _dbContext.Doges.Where(d => d.WentHome == request.WentHome).CountAsync(cancellationToken);

            List<Dog> dogs = await _dbContext.Doges.AnyAsync(cancellationToken)
                ? await _dbContext.Doges
                    .Where(d => d.WentHome == request.WentHome)
                    .OrderByDescending(d => d.Id)
                    .Skip((request.Page - 1) * request.DogsPerPage)
                    .Take(request.DogsPerPage)
                    .ToListAsync(cancellationToken)
                : (new());

            return new GetDogsByWentHomeQueryResult
            {
                Dogs = dogs,
                DogsCount = dogsCount
            };
        }
    }
}