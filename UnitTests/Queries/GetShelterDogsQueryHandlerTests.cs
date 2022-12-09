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
    public class GetShelterDogsQueryHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<GetShelterDogsQuery, GetShelterDogsQueryResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public GetShelterDogsQueryHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new GetShelterDogsQueryHandler(_dbContext);

        }

        [Fact]
        public async Task QueryShouldReturnDog()
        {
            // Arragne
            int homeDogsCount = _random.Next(10);

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
            }

            int shelterDogsCount = _random.Next(10);
            List<Dog> shelterDogs = new(shelterDogsCount);
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
                shelterDogs.Add(dog);
            }

            GetShelterDogsQuery getShelterDogsQuery = new();

            // Act
            GetShelterDogsQueryResult result = await _handler.Handle(getShelterDogsQuery, CancellationToken.None);

            // Assert
            List<Dog> resultDogs = result.Dogs.OrderBy(d => d.Id).ToList();
            resultDogs.Count.ShouldBeEquivalentTo(shelterDogs.Count);
            for (int i = 0; i < shelterDogs.Count; ++i)
            {
                shelterDogs[i].Name.ShouldBeEquivalentTo(resultDogs[i].Name);
                shelterDogs[i].Breed.ShouldBeEquivalentTo(resultDogs[i].Breed);
                shelterDogs[i].Size.ShouldBeEquivalentTo(resultDogs[i].Size);
                shelterDogs[i].BirthDate.ShouldBeEquivalentTo(resultDogs[i].BirthDate);
                shelterDogs[i].About.ShouldBeEquivalentTo(resultDogs[i].About);
                shelterDogs[i].Row.ShouldBeEquivalentTo(resultDogs[i].Row);
                shelterDogs[i].Enclosure.ShouldBeEquivalentTo(resultDogs[i].Enclosure);
            }
        }

        [Fact]
        public async Task QueryShouldReturnNullIfDogsNotFound()
        {
            // Arragne
            int homeDogsCount = _random.Next(10);

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
            }

            GetShelterDogsQuery getShelterDogsQuery = new();

            // Act
            GetShelterDogsQueryResult result = await _handler.Handle(getShelterDogsQuery, CancellationToken.None);

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