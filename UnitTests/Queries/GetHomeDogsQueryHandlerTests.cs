using System;
using System.Collections.Generic;
using System.Linq;
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
    public class GetHomeDogsQueryHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<GetHomeDogsQuery, GetHomeDogsQueryResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public GetHomeDogsQueryHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new GetHomeDogsQueryHandler(_dbContext);

        }

        [Fact]
        public async Task QueryShouldReturnDog()
        {
            // Arragne
            int homeDogsCount = _random.Next(10);
            List<Dog> homeDogs = new(homeDogsCount);

            for (int i = 0; i < homeDogsCount; ++i)
            {
                Dog dog = new()
                {
                    Name = Guid.NewGuid().ToString(),
                    Breed = Guid.NewGuid().ToString(),
                    Size = _sizes[_random.Next(_sizes.Length)],
                    BirthDate = RandomDate.GetRandomDate().ToUniversalTime(),
                    About = Guid.NewGuid().ToString(),
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

            for (int i = 0; i < shelterDogsCount; ++i)
            {
                Dog dog = new()
                {
                    Name = Guid.NewGuid().ToString(),
                    Breed = Guid.NewGuid().ToString(),
                    Size = _sizes[_random.Next(_sizes.Length)],
                    BirthDate = RandomDate.GetRandomDate().ToUniversalTime(),
                    About = Guid.NewGuid().ToString(),
                    Row = _random.Next(100),
                    Enclosure = _random.Next(100),
                    UpdatedBy = Guid.NewGuid().ToString()
                };
                _ = await _dbContext.AddAsync(dog);
                _ = await _dbContext.SaveChangesAsync();
            }

            GetHomeDogsQuery getHomeDogsQuery = new();

            // Act
            GetHomeDogsQueryResult result = await _handler.Handle(getHomeDogsQuery, CancellationToken.None);

            // Assert
            List<Dog> resultDogs = result.Dogs.OrderBy(d => d.Id).ToList();
            resultDogs.Count.ShouldBeEquivalentTo(homeDogs.Count);
            for (int i = 0; i < homeDogs.Count; ++i)
            {
                homeDogs[i].Name.ShouldBeEquivalentTo(resultDogs[i].Name);
                homeDogs[i].Breed.ShouldBeEquivalentTo(resultDogs[i].Breed);
                homeDogs[i].Size.ShouldBeEquivalentTo(resultDogs[i].Size);
                homeDogs[i].BirthDate.ShouldBeEquivalentTo(resultDogs[i].BirthDate);
                homeDogs[i].About.ShouldBeEquivalentTo(resultDogs[i].About);
                homeDogs[i].Row.ShouldBeEquivalentTo(resultDogs[i].Row);
                homeDogs[i].Enclosure.ShouldBeEquivalentTo(resultDogs[i].Enclosure);
            }
        }

        [Fact]
        public async Task QueryShouldReturnNullIfDogsNotFound()
        {
            // Arragne
            int shelterDogsCount = _random.Next(10);

            for (int i = 0; i < shelterDogsCount; ++i)
            {
                Dog dog = new()
                {
                    Name = Guid.NewGuid().ToString(),
                    Breed = Guid.NewGuid().ToString(),
                    Size = _sizes[_random.Next(_sizes.Length)],
                    BirthDate = RandomDate.GetRandomDate().ToUniversalTime(),
                    About = Guid.NewGuid().ToString(),
                    Row = _random.Next(100),
                    Enclosure = _random.Next(100),
                    UpdatedBy = Guid.NewGuid().ToString()
                };
                _ = await _dbContext.AddAsync(dog);
                _ = await _dbContext.SaveChangesAsync();
            }

            GetHomeDogsQuery getHomeDogsQuery = new();

            // Act
            GetHomeDogsQueryResult result = await _handler.Handle(getHomeDogsQuery, CancellationToken.None);

            // Assert
            result.Dogs.ShouldBeEmpty();
        }

        public void Dispose()
        {
            _ = _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}