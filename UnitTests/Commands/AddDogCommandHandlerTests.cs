using System;
using System.Threading;
using System.Threading.Tasks;

using Domain.Commands;
using Domain.Database;

using MediatR;

using Shouldly;

using UnitTests.Helpers;

namespace UnitTests.Commands
{
    public class AddDogCommandHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<AddDogCommand, AddDogCommandResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public AddDogCommandHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new AddDogCommandHandler(_dbContext);

        }

        [Fact]
        public async Task DogShouldBeAdded()
        {
            // Arragne
            string newName = Guid.NewGuid().ToString();
            string newBreed = Guid.NewGuid().ToString();
            string newSize = _sizes[_random.Next(_sizes.Length)];
            DateTime newBirthDate = RandomDate.GetRandomDate();
            string newAbout = Guid.NewGuid().ToString();
            int newRow = _random.Next(100);
            int newEnclosure = _random.Next(100);
            string user = Guid.NewGuid().ToString();
            AddDogCommand command = new()
            {
                Name = newName,
                Breed = newBreed,
                Size = newSize,
                BirthDate = newBirthDate,
                About = newAbout,
                Row = newRow,
                Enclosure = newEnclosure,
                UpdatedBy = user
            };

            // Act
            AddDogCommandResult result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Dog.Id.ShouldBePositive();
            result.Dog.Name.ShouldBeEquivalentTo(newName);
            result.Dog.Breed.ShouldBeEquivalentTo(newBreed);
            result.Dog.Size.ShouldBeEquivalentTo(newSize);
            result.Dog.BirthDate.ShouldBeEquivalentTo(newBirthDate.ToUniversalTime());
            result.Dog.About.ShouldBeEquivalentTo(newAbout);
            result.Dog.Row.ShouldBeEquivalentTo(newRow);
            result.Dog.Enclosure.ShouldBeEquivalentTo(newEnclosure);
            result.Dog.WentHome.ShouldBeFalse();
            result.Dog.UpdatedBy.ShouldBeEquivalentTo(user);
        }

        [Fact]
        public async Task DogShouldNotBeAddedIfUserNotAuthorized()
        {
            // Arragne
            string newName = Guid.NewGuid().ToString();
            string newBreed = Guid.NewGuid().ToString();
            string newSize = _sizes[_random.Next(_sizes.Length)];
            DateTime newBirthDate = RandomDate.GetRandomDate();
            string newAbout = Guid.NewGuid().ToString();
            int newRow = _random.Next(100);
            int newEnclosure = _random.Next(100);
            AddDogCommand command = new()
            {
                Name = newName,
                Breed = newBreed,
                Size = newSize,
                BirthDate = newBirthDate,
                About = newAbout,
                Row = newRow,
                Enclosure = newEnclosure
            };

            // Act
            AddDogCommandResult result = await _handler.Handle(command, CancellationToken.None);

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