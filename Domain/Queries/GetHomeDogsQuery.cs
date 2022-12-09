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
            List<Dog> allDogs = await _dbContext.Doges.AnyAsync(cancellationToken)
                ? await _dbContext.Doges
                    .Where(d => d.WentHome)
                    .OrderByDescending(d => d.Id)
                    .ToListAsync(cancellationToken)
                : (new());

            return new GetHomeDogsQueryResult
            {
                Dogs = allDogs
            };
        }
    }
}