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
    public class GetDogsBirthDatesHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<GetDogsBirthDatesQuery, GetDogsBirthDatesQueryResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public GetDogsBirthDatesHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new GetDogsBirthDatesQueryHandler(_dbContext);

        }

        [Fact]
        public async Task QueryShouldReturnBirthDates()
        {
            // Arragne
            int homeDogsCount = _random.Next(10);
            List<DateTime> homeDogsBirthDates = new(homeDogsCount);

            for (int i = 0; i < homeDogsCount; ++i)
            {
                DateTime birthDate = RandomDate.GetRandomDate().ToUniversalTime();
                Dog dog = new()
                {
                    Name = Guid.NewGuid().ToString(),
                    Breed = Guid.NewGuid().ToString(),
                    Size = _sizes[_random.Next(_sizes.Length)],
                    BirthDate = birthDate,
                    About = Guid.NewGuid().ToString(),
                    Row = _random.Next(100),
                    Enclosure = _random.Next(100),
                    WentHome = true,
                    UpdatedBy = Guid.NewGuid().ToString()
                };
                _ = await _dbContext.AddAsync(dog);
                _ = await _dbContext.SaveChangesAsync();
                homeDogsBirthDates.Add(birthDate);
            }

            int shelterDogsCount = _random.Next(10);
            List<DateTime> shelterDogsBirthDates = new(shelterDogsCount);

            for (int i = 0; i < shelterDogsCount; ++i)
            {
                DateTime birthDate = RandomDate.GetRandomDate().ToUniversalTime();
                Dog dog = new()
                {
                    Name = Guid.NewGuid().ToString(),
                    Breed = Guid.NewGuid().ToString(),
                    Size = _sizes[_random.Next(_sizes.Length)],
                    BirthDate = birthDate,
                    About = Guid.NewGuid().ToString(),
                    Row = _random.Next(100),
                    Enclosure = _random.Next(100),
                    UpdatedBy = Guid.NewGuid().ToString()
                };
                _ = await _dbContext.AddAsync(dog);
                _ = await _dbContext.SaveChangesAsync();
                shelterDogsBirthDates.Add(birthDate);
            }

            GetDogsBirthDatesQuery getHomeDogsBirthDatesQuery = new()
            {
                WentHome = true
            };

            GetDogsBirthDatesQuery getShelterDogsBirthDatesQuery = new()
            {
                WentHome = false
            };

            // Act
            GetDogsBirthDatesQueryResult homeResult = await _handler.Handle(getHomeDogsBirthDatesQuery, CancellationToken.None);
            GetDogsBirthDatesQueryResult shelterResult = await _handler.Handle(getShelterDogsBirthDatesQuery, CancellationToken.None);

            // Assert
            homeDogsBirthDates.ShouldBe(homeResult.DogsBirthDates);
            shelterDogsBirthDates.ShouldBe(shelterResult.DogsBirthDates);

        }

        public void Dispose()
        {
            _ = _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}