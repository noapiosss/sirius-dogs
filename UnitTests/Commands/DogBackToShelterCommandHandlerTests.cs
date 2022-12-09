using Microsoft.EntityFrameworkCore;

using Contracts.Database;

using Domain.Commands;
using Domain.Database;

using MediatR;

using Shouldly;

using UnitTests.Helpers;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace UnitTests.Commands
{
    public class DogBackToShelterCommandHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<DogBackToShelterCommand, DogBackToShelterCommandResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public DogBackToShelterCommandHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new DogBackToShelterCommandHandler(_dbContext);

        }

        [Fact]
        public async Task IfWentHomeIsTrueStatusShouldBeChanged()
        {
            // Arragne
            string name = Guid.NewGuid().ToString();
            string breed = Guid.NewGuid().ToString();
            string size = _sizes[_random.Next(_sizes.Length)];
            DateTime birthDate = RandomDate.GetRandomDate();
            string about = Guid.NewGuid().ToString();
            int row = _random.Next(100);
            int enclosure = _random.Next(100);
            DateTime lastUpdate = DateTime.Now;
            string user = Guid.NewGuid().ToString();
            Dog dog = new()
            {
                Name = name,
                Breed = breed,
                Size = size,
                BirthDate = birthDate,
                About = about,
                Row = row,
                Enclosure = enclosure,
                LastUpdate = lastUpdate,
                WentHome = true,
                UpdatedBy = user
            };
            _ = await _dbContext.Doges.AddAsync(dog);
            _ = await _dbContext.SaveChangesAsync();

            string updatedBy = Guid.NewGuid().ToString();

            DogBackToShelterCommand dogBackToShelterCommand = new()
            {
                DogId = dog.Id,
                UpdatedBy = updatedBy
            };

            // Act
            DogBackToShelterCommandResult result = await _handler.Handle(dogBackToShelterCommand, CancellationToken.None);

            // Assert
            result.StatusIsChanged.ShouldBeTrue();
            (await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == dog.Id, CancellationToken.None)).WentHome.ShouldBeFalse();
            (await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == dog.Id, CancellationToken.None)).UpdatedBy.ShouldBeEquivalentTo(updatedBy);
        }

        [Fact]
        public async Task IfWentHomeIsFalseStatusShouldNotBeChanged()
        {
            // Arragne
            string name = Guid.NewGuid().ToString();
            string breed = Guid.NewGuid().ToString();
            string size = _sizes[_random.Next(_sizes.Length)];
            DateTime birthDate = RandomDate.GetRandomDate();
            string about = Guid.NewGuid().ToString();
            int row = _random.Next(100);
            int enclosure = _random.Next(100);
            DateTime lastUpdate = DateTime.Now;
            string user = Guid.NewGuid().ToString();
            Dog dog = new()
            {
                Name = name,
                Breed = breed,
                Size = size,
                BirthDate = birthDate,
                About = about,
                Row = row,
                Enclosure = enclosure,
                LastUpdate = lastUpdate,
                UpdatedBy = user
            };
            _ = await _dbContext.Doges.AddAsync(dog);
            _ = await _dbContext.SaveChangesAsync();

            string updatedBy = Guid.NewGuid().ToString();

            DogBackToShelterCommand dogBackToShelterCommand = new()
            {
                DogId = dog.Id,
                UpdatedBy = updatedBy
            };


            // Act
            DogBackToShelterCommandResult result = await _handler.Handle(dogBackToShelterCommand, CancellationToken.None);

            // Assert
            result.StatusIsChanged.ShouldBeFalse();
            result.Comment.ShouldBeEquivalentTo("Dog is already in shelter");
            (await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == dog.Id, CancellationToken.None)).WentHome.ShouldBeFalse();
        }

        [Fact]
        public async Task WentHomeStatusShouldNotBeChangedIfUnauthorized()
        {
            // Arragne
            string name = Guid.NewGuid().ToString();
            string breed = Guid.NewGuid().ToString();
            string size = _sizes[_random.Next(_sizes.Length)];
            DateTime birthDate = RandomDate.GetRandomDate();
            string about = Guid.NewGuid().ToString();
            int row = _random.Next(100);
            int enclosure = _random.Next(100);
            DateTime lastUpdate = DateTime.Now;
            string user = Guid.NewGuid().ToString();
            Dog dog = new()
            {
                Name = name,
                Breed = breed,
                Size = size,
                BirthDate = birthDate,
                About = about,
                Row = row,
                Enclosure = enclosure,
                WentHome = true,
                LastUpdate = lastUpdate,
                UpdatedBy = user
            };
            _ = await _dbContext.Doges.AddAsync(dog);
            _ = await _dbContext.SaveChangesAsync();

            DogBackToShelterCommand dogBackToShelter = new()
            {
                DogId = dog.Id,
                UpdatedBy = ""
            };

            // Act
            DogBackToShelterCommandResult result = await _handler.Handle(dogBackToShelter, CancellationToken.None);

            // Assert
            result.StatusIsChanged.ShouldBeFalse();
            result.Comment.ShouldBeEquivalentTo("Unauthorized");
            (await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == dog.Id, CancellationToken.None)).WentHome.ShouldBeTrue();
        }

        [Fact]
        public async Task WentHomeStatusShouldNotBeChangedIfDogNotExists()
        {
            // Arragne
            string name = Guid.NewGuid().ToString();
            string breed = Guid.NewGuid().ToString();
            string size = _sizes[_random.Next(_sizes.Length)];
            DateTime birthDate = RandomDate.GetRandomDate();
            string about = Guid.NewGuid().ToString();
            int row = _random.Next(100);
            int enclosure = _random.Next(100);
            DateTime lastUpdate = DateTime.Now;
            string user = Guid.NewGuid().ToString();
            Dog dog = new()
            {
                Name = name,
                Breed = breed,
                Size = size,
                BirthDate = birthDate,
                About = about,
                Row = row,
                Enclosure = enclosure,
                WentHome = true,
                LastUpdate = lastUpdate,
                UpdatedBy = user
            };
            _ = await _dbContext.Doges.AddAsync(dog);
            _ = await _dbContext.SaveChangesAsync();

            string updatedBy = Guid.NewGuid().ToString();

            DogBackToShelterCommand dogBackToShelterCommand = new()
            {
                DogId = -1,
                UpdatedBy = updatedBy
            };

            // Act
            DogBackToShelterCommandResult result = await _handler.Handle(dogBackToShelterCommand, CancellationToken.None);

            // Assert
            result.StatusIsChanged.ShouldBeFalse();
            result.Comment.ShouldBeEquivalentTo("Dog not found");
        }

        public void Dispose()
        {
            _ = _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}