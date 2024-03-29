using System;
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
    public class SearchDogQuery : IRequest<SearchDogQueryResult>
    {
        public string SearchRequest { get; init; }
        public string Gender { get; init; }
        public string Size { get; init; }
        public int MaxAge { get; init; }
        public int? Row { get; init; }
        public int? Enclosure { get; init; }
        public bool WentHome { get; init; }
        public int DogsPerPage { get; init; }
        public int Page { get; init; }
    }

    public class SearchDogQueryResult
    {
        public ICollection<Dog> Dogs { get; init; }
        public int DogsCount { get; init; }
    }

    internal class SearchDogQueryHandler : IRequestHandler<SearchDogQuery, SearchDogQueryResult>
    {
        private readonly DogesDbContext _dbContext;


        public SearchDogQueryHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<SearchDogQueryResult> Handle(SearchDogQuery request, CancellationToken cancellationToken)
        {
            string searchRequest = string.IsNullOrEmpty(request.SearchRequest) ? "" : request.SearchRequest.ToLower();
            DateTime minBirthDate = DateTime.UtcNow.AddMonths(-request.MaxAge);
            minBirthDate = minBirthDate.AddDays(-minBirthDate.Day);

            int dogsCount = await _dbContext.Doges
                .Where(d => d.WentHome == request.WentHome &&
                    d.BirthDate.Date >= minBirthDate.Date &&
                    (request.Row == null || d.Row == request.Row) &&
                    (request.Enclosure == null || d.Enclosure == request.Enclosure) &&
                    (request.Gender == "Any" || d.Gender == request.Gender) &&
                    (request.Size == "Any" || d.Size == request.Size) &&
                    (EF.Functions.Like(d.Name.ToLower(), $"%{searchRequest}%") ||
                    EF.Functions.Like(d.Breed.ToLower(), $"%{searchRequest}%") ||
                    EF.Functions.Like(d.About.ToLower(), $"%{searchRequest}%")))
                .CountAsync(cancellationToken);

            List<Dog> dogs = await _dbContext.Doges
                .Where(d => d.WentHome == request.WentHome &&
                    d.BirthDate.Date >= minBirthDate.Date &&
                    (request.Row == null || d.Row == request.Row) &&
                    (request.Enclosure == null || d.Enclosure == request.Enclosure) &&
                    (request.Gender == "Any" || d.Gender == request.Gender) &&
                    (request.Size == "Any" || d.Size == request.Size) &&
                    (EF.Functions.Like(d.Name.ToLower(), $"%{searchRequest}%") ||
                    EF.Functions.Like(d.Breed.ToLower(), $"%{searchRequest}%") ||
                    EF.Functions.Like(d.About.ToLower(), $"%{searchRequest}%")))
                .OrderByDescending(d => d.Id)
                .Skip((request.Page - 1) * request.DogsPerPage)
                .Take(request.DogsPerPage)
                .ToListAsync(cancellationToken);

            return new SearchDogQueryResult
            {
                Dogs = dogs,
                DogsCount = dogsCount
            };
        }
    }
}