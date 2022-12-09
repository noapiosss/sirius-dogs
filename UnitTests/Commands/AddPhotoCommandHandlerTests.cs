using Microsoft.EntityFrameworkCore;

using Contracts.Database;

using Domain.Commands;
using Domain.Database;

using MediatR;

using Shouldly;

using UnitTests.Helpers;
using System.Threading;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace UnitTests.Commands
{
    public class AddPhotoCommandHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<AddPhotoCommand, AddPhotoCommandResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public AddPhotoCommandHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new AddPhotoCommandHandler(_dbContext);

        }

        [Fact]
        public async Task PhotoShouldBeAdded()
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
            _ = await _dbContext.AddAsync(dog);
            _ = await _dbContext.SaveChangesAsync();

            string newPhotoUrl = Guid.NewGuid().ToString();
            string newUser = Guid.NewGuid().ToString();

            AddPhotoCommand addPhotoCommand = new()
            {
                DogId = dog.Id,
                PhotoUrl = newPhotoUrl,
                UpdatedBy = newUser
            };

            // Act
            AddPhotoCommandResult result = await _handler.Handle(addPhotoCommand, CancellationToken.None);

            // Assert
            result.PhotoUrl.ShouldBeEquivalentTo(newPhotoUrl);

            Dog currentDogInDb = await _dbContext.Doges
                .Include(d => d.Photos)
                .FirstOrDefaultAsync(CancellationToken.None);
            currentDogInDb.LastUpdate.ShouldNotBeSameAs(lastUpdate);
            currentDogInDb.UpdatedBy.ShouldBeEquivalentTo(newUser);
            currentDogInDb.Photos.Select(p => p.PhotoPath).ShouldContain(result.PhotoUrl);
        }

        [Fact]
        public async Task PhotoShouldNotBeAddedIfNotAuthorized()
        {
            // Arragne
            string name = Guid.NewGuid().ToString();
            string breed = Guid.NewGuid().ToString();
            string size = _sizes[_random.Next(_sizes.Length)];
            DateTime birthDate = RandomDate.GetRandomDate();
            string about = Guid.NewGuid().ToString();
            int row = _random.Next(100);
            int enclosure = _random.Next(100);
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
                LastUpdate = DateTime.Now,
                UpdatedBy = user
            };
            _ = await _dbContext.Doges.AddAsync(dog);
            _ = await _dbContext.SaveChangesAsync();

            string newPhotoUrl = Guid.NewGuid().ToString();
            string newUser = "";

            AddPhotoCommand addPhotoCommand = new()
            {
                DogId = dog.Id,
                PhotoUrl = newPhotoUrl,
                UpdatedBy = newUser
            };

            // Act
            AddPhotoCommandResult result = await _handler.Handle(addPhotoCommand, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task PhotoShouldNotBeAddedIfPhotoUrlIsEmpty()
        {
            // Arragne
            string name = Guid.NewGuid().ToString();
            string breed = Guid.NewGuid().ToString();
            string size = _sizes[_random.Next(_sizes.Length)];
            DateTime birthDate = RandomDate.GetRandomDate();
            string about = Guid.NewGuid().ToString();
            int row = _random.Next(100);
            int enclosure = _random.Next(100);
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
                LastUpdate = DateTime.Now,
                UpdatedBy = user
            };
            _ = await _dbContext.Doges.AddAsync(dog);
            _ = await _dbContext.SaveChangesAsync();

            string newPhotoUrl = "";
            string newUser = Guid.NewGuid().ToString();

            AddPhotoCommand addPhotoCommand = new()
            {
                DogId = dog.Id,
                PhotoUrl = newPhotoUrl,
                UpdatedBy = newUser
            };

            // Act
            AddPhotoCommandResult result = await _handler.Handle(addPhotoCommand, CancellationToken.None);

            // Assert
            result.ShouldBeNull();
        }

        public void Dispose()
        {
            _ = _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}