using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Queries
{
    public class GetDogsBirthDatesQuery : IRequest<GetDogsBirthDatesQueryResult>
    {
        public bool WentHome { get; init; }
    }

    public class GetDogsBirthDatesQueryResult
    {
        public ICollection<DateTime> DogsBirthDates { get; init; }
    }

    internal class GetDogsBirthDatesQueryHandler : IRequestHandler<GetDogsBirthDatesQuery, GetDogsBirthDatesQueryResult>
    {
        private readonly DogesDbContext _dbContext;


        public GetDogsBirthDatesQueryHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<GetDogsBirthDatesQueryResult> Handle(GetDogsBirthDatesQuery request, CancellationToken cancellationToken)
        {
            List<DateTime> dogsBirthDates = await _dbContext.Doges.AnyAsync(cancellationToken)
                    ? await _dbContext.Doges
                        .Where(d => d.WentHome == request.WentHome)
                        .Select(d => d.BirthDate)
                        .ToListAsync(cancellationToken)
                    : (new());

            return new GetDogsBirthDatesQueryResult
            {
                DogsBirthDates = dogsBirthDates
            };
        }
    }
}