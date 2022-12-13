using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;

using Domain.Database;
using Domain.Queries;

using MediatR;

using Shouldly;

using UnitTests.Helpers;

namespace unittests.Queries
{
    public class SearchDogQueryHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<SearchDogQuery, SearchDogQueryResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public SearchDogQueryHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new SearchDogQueryHandler(_dbContext);

        }

        [Fact]
        public async Task QueryShouldReturnBirthDates()
        {
            // Arragne
            string searchRequest = Guid.NewGuid().ToString()[..5]; // not sure about this
            int homeDogsCount = _random.Next(10);
            List<Dog> homeDogs = new(homeDogsCount);

            for (int i = 0; i < homeDogsCount; ++i)
            {
                Dog dog = new()
                {
                    Name = _random.Next(100) < 20 ? Guid.NewGuid().ToString().Insert(_random.Next(36), searchRequest) : Guid.NewGuid().ToString(),
                    Breed = _random.Next(100) < 20 ? Guid.NewGuid().ToString().Insert(_random.Next(36), searchRequest) : Guid.NewGuid().ToString(),
                    Size = _sizes[_random.Next(_sizes.Length)],
                    BirthDate = RandomDate.GetRandomDate().ToUniversalTime(),
                    About = _random.Next(100) < 20 ? Guid.NewGuid().ToString().Insert(_random.Next(36), searchRequest) : Guid.NewGuid().ToString(),
                    Row = _random.Next(100),
                    Enclosure = _random.Next(100),
                    WentHome = true,
                    UpdatedBy = Guid.NewGuid().ToString()
                };
                _ = await _dbContext.AddAsync(dog);
                _ = await _dbContext.SaveChangesAsync();
                homeDogs.Add(dog);
            }

            int shelterDogsCount = _random.Next(10);
            List<Dog> shelterDogs = new(shelterDogsCount);

            for (int i = 0; i < shelterDogsCount; ++i)
            {
                Dog dog = new()
                {
                    Name = _random.Next(100) < 20 ? Guid.NewGuid().ToString().Insert(_random.Next(36), searchRequest) : Guid.NewGuid().ToString(),
                    Breed = _random.Next(100) < 20 ? Guid.NewGuid().ToString().Insert(_random.Next(36), searchRequest) : Guid.NewGuid().ToString(),
                    Size = _sizes[_random.Next(_sizes.Length)],
                    BirthDate = RandomDate.GetRandomDate().ToUniversalTime(),
                    About = _random.Next(100) < 20 ? Guid.NewGuid().ToString().Insert(_random.Next(36), searchRequest) : Guid.NewGuid().ToString(),
                    Row = _random.Next(100),
                    Enclosure = _random.Next(100),
                    UpdatedBy = Guid.NewGuid().ToString()
                };
                _ = await _dbContext.AddAsync(dog);
                _ = await _dbContext.SaveChangesAsync();
                shelterDogs.Add(dog);
            }

            bool searchWentHome = _random.Next(100) < 50;
            int maxAge;
            int row;
            int enclosure;

            if (searchWentHome)
            {
                maxAge = GetAgeInMonthHelper.GetAgeInMonth(homeDogs[_random.Next(homeDogsCount)].BirthDate);
                row = homeDogs[_random.Next(homeDogsCount)].Row;
                enclosure = homeDogs[_random.Next(homeDogsCount)].Enclosure;
            }
            else
            {
                maxAge = GetAgeInMonthHelper.GetAgeInMonth(shelterDogs[_random.Next(shelterDogsCount)].BirthDate);
                row = shelterDogs[_random.Next(shelterDogsCount)].Row;
                enclosure = shelterDogs[_random.Next(shelterDogsCount)].Enclosure;
            }

            SearchDogQuery searchDogQuery = new()
            {
                SearchRequest = searchRequest,
                MaxAge = maxAge,
                Row = row,
                Enclosure = enclosure,
                WentHome = searchWentHome
            };

            // Act
            SearchDogQueryResult searchResult = await _handler.Handle(searchDogQuery, CancellationToken.None);

            // Assert
            foreach (Dog dog in searchResult.Dogs)
            {
                GetAgeInMonthHelper.GetAgeInMonth(dog.BirthDate).ShouldBeLessThanOrEqualTo(maxAge);
                dog.Row.ShouldBeEquivalentTo(row);
                dog.Enclosure.ShouldBeEquivalentTo(enclosure);
                dog.WentHome.ShouldBeEquivalentTo(searchWentHome);
            }
        }

        public void Dispose()
        {
            _ = _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}