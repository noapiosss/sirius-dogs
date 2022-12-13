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
    public class GetDogBirthDatesQuery : IRequest<GetDogBirthDatesQueryResult>
    {
        public bool WentHome { get; init; }
    }

    public class GetDogBirthDatesQueryResult
    {
        public ICollection<DateTime> DogsBirthDates { get; init; }
    }

    internal class GetDogBirthDatesQueryHandler : IRequestHandler<GetDogBirthDatesQuery, GetDogBirthDatesQueryResult>
    {
        private readonly DogesDbContext _dbContext;


        public GetDogBirthDatesQueryHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<GetDogBirthDatesQueryResult> Handle(GetDogBirthDatesQuery request, CancellationToken cancellationToken)
        {
            List<DateTime> dogsBirthDates = await _dbContext.Doges.AnyAsync(cancellationToken)
                    ? await _dbContext.Doges
                        .Where(d => d.WentHome == request.WentHome)
                        .Select(d => d.BirthDate)
                        .ToListAsync(cancellationToken)
                    : (new());

            return new GetDogBirthDatesQueryResult
            {
                DogsBirthDates = dogsBirthDates
            };
        }
    }
}