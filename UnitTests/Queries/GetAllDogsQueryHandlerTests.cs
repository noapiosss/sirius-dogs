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
    public class GetAllDogsQueryHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<GetAllDogsQuery, GetAllDogsQueryResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public GetAllDogsQueryHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new GetAllDogsQueryHandler(_dbContext);

        }

        [Fact]
        public async Task QueryShouldReturnAllDogs()
        {
            // Arragne
            int dogsCount = _random.Next(10);
            List<Dog> dogs = new(dogsCount);

            for (int i = 0; i < dogsCount; ++i)
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
                dogs.Add(dog);
                _ = await _dbContext.AddAsync(dog);
                _ = await _dbContext.SaveChangesAsync();
            }

            GetAllDogsQuery getAllDogsQuery = new();

            // Act
            GetAllDogsQueryResult result = await _handler.Handle(getAllDogsQuery, CancellationToken.None);

            // Assert
            List<Dog> resultDogs = result.Dogs.OrderBy(d => d.Id).ToList();
            resultDogs.Count.ShouldBeEquivalentTo(dogs.Count);
            for (int i = 0; i < dogs.Count; ++i)
            {
                dogs[i].Name.ShouldBeEquivalentTo(resultDogs[i].Name);
                dogs[i].Breed.ShouldBeEquivalentTo(resultDogs[i].Breed);
                dogs[i].Size.ShouldBeEquivalentTo(resultDogs[i].Size);
                dogs[i].BirthDate.ShouldBeEquivalentTo(resultDogs[i].BirthDate);
                dogs[i].About.ShouldBeEquivalentTo(resultDogs[i].About);
                dogs[i].Row.ShouldBeEquivalentTo(resultDogs[i].Row);
                dogs[i].Enclosure.ShouldBeEquivalentTo(resultDogs[i].Enclosure);
            }

        }

        public void Dispose()
        {
            _ = _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}