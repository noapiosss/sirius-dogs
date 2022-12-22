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
    public class TelegramSearchDogQuery : IRequest<TelegramSearchDogQueryResult>
    {
        public string SearchRequest { get; init; }
        public int Page { get; init; }
    }

    public class TelegramSearchDogQueryResult
    {
        public ICollection<Dog> Dogs { get; init; }
    }

    internal class TelegramSearchDogQueryHandler : IRequestHandler<TelegramSearchDogQuery, TelegramSearchDogQueryResult>
    {
        private readonly DogesDbContext _dbContext;


        public TelegramSearchDogQueryHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<TelegramSearchDogQueryResult> Handle(TelegramSearchDogQuery request, CancellationToken cancellationToken)
        {
            string searchRequest = string.IsNullOrEmpty(request.SearchRequest) ? "" : request.SearchRequest.ToLower();

            List<Dog> dogs = await _dbContext.Doges
                .Where(d => !d.WentHome &&
                    (EF.Functions.Like(d.Name.ToLower(), $"%{searchRequest}%") ||
                    EF.Functions.Like(d.Breed.ToLower(), $"%{searchRequest}%") ||
                    EF.Functions.Like(d.Size.ToLower(), $"%{searchRequest}%") ||
                    EF.Functions.Like(d.About.ToLower(), $"%{searchRequest}%")))
                .Include(d => d.Photos)
                .ToListAsync(cancellationToken);

            return new TelegramSearchDogQueryResult
            {
                Dogs = dogs
            };
        }
    }
}