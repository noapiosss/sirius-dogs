using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Queries
{
    public class GetMinMaxAgeQuery : IRequest<GetMinMaxAgeQueryResult>
    {

    }

    public class GetMinMaxAgeQueryResult
    {
        public DateTime MinBirthDate { get; init; }
        public DateTime MaxBirthDate { get; init; }
    }

    internal class GetMinMaxAgeQueryHandler : IRequestHandler<GetMinMaxAgeQuery, GetMinMaxAgeQueryResult>
    {
        private readonly DogesDbContext _dbContext;


        public GetMinMaxAgeQueryHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<GetMinMaxAgeQueryResult> Handle(GetMinMaxAgeQuery request, CancellationToken cancellationToken)
        {
            DateTime minBirthDate = await _dbContext.Doges.AnyAsync(cancellationToken) ? _dbContext.Doges.Select(d => d.BirthDate).Min() : DateTime.Now;
            DateTime maxBirthDate = await _dbContext.Doges.AnyAsync(cancellationToken) ? _dbContext.Doges.Select(d => d.BirthDate).Max() : DateTime.Now;

            return new GetMinMaxAgeQueryResult
            {
                MinBirthDate = minBirthDate,
                MaxBirthDate = maxBirthDate
            };
        }
    }
}