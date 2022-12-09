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
    public class GetDogByIdQueryHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<GetDogByIdQuery, GetDogByIdQueryResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public GetDogByIdQueryHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new GetDogByIdQueryHandler(_dbContext);

        }

        [Fact]
        public async Task QueryShouldReturnDog()
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
                _ = await _dbContext.AddAsync(dog);
                _ = await _dbContext.SaveChangesAsync();
                dogs.Add(dog);
            }

            int randomDogId = dogs[_random.Next(dogsCount)].Id;
            GetDogByIdQuery getDogByIdQuery = new()
            {
                DogId = randomDogId
            };

            // Act
            GetDogByIdQueryResult result = await _handler.Handle(getDogByIdQuery, CancellationToken.None);

            // Assert
            Dog initialDog = dogs.First(d => d.Id == randomDogId);
            result.Dog.Name.ShouldBeEquivalentTo(initialDog.Name);
            result.Dog.Breed.ShouldBeEquivalentTo(initialDog.Breed);
            result.Dog.Size.ShouldBeEquivalentTo(initialDog.Size);
            result.Dog.BirthDate.ShouldBeEquivalentTo(initialDog.BirthDate);
            result.Dog.About.ShouldBeEquivalentTo(initialDog.About);
            result.Dog.Row.ShouldBeEquivalentTo(initialDog.Row);
            result.Dog.Enclosure.ShouldBeEquivalentTo(initialDog.Enclosure);
        }

        [Fact]
        public async Task QueryShouldReturnNullIfDogNotFound()
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
                _ = await _dbContext.AddAsync(dog);
                _ = await _dbContext.SaveChangesAsync();
                dogs.Add(dog);
            }

            GetDogByIdQuery getDogByIdQuery = new()
            {
                DogId = -1
            };

            // Act
            GetDogByIdQueryResult result = await _handler.Handle(getDogByIdQuery, CancellationToken.None);

            // Assert
            result.Dog.ShouldBeNull();
        }

        public void Dispose()
        {
            _ = _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}