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
    public class EditDogCommandHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<EditDogCommand, EditDogCommandResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public EditDogCommandHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new EditDogCommandHandler(_dbContext);

        }

        [Fact]
        public async Task DogInfoShouldBeChanged()
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

            string newName = Guid.NewGuid().ToString();
            string newBreed = Guid.NewGuid().ToString();
            string newSize = _sizes[_random.Next(_sizes.Length)];
            DateTime newBirthDate = RandomDate.GetRandomDate();
            string newAbout = Guid.NewGuid().ToString();
            int newRow = _random.Next(100);
            int newEnclosure = _random.Next(100);
            string updatedBy = Guid.NewGuid().ToString();
            Dog newDog = new()
            {
                Id = dog.Id,
                Name = newName,
                Breed = newBreed,
                Size = newSize,
                BirthDate = newBirthDate,
                About = newAbout,
                Row = newRow,
                Enclosure = newEnclosure
            };

            EditDogCommand editDogCommand = new()
            {
                Dog = newDog,
                UpdatedBy = updatedBy
            };

            // Act
            EditDogCommandResult result = await _handler.Handle(editDogCommand, CancellationToken.None);

            // Assert
            Dog editedDog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == dog.Id, CancellationToken.None);
            result.Dog.ShouldBe(editedDog);
            editedDog.Name.ShouldBeEquivalentTo(newName);
            editedDog.Breed.ShouldBeEquivalentTo(newBreed);
            editedDog.Size.ShouldBeEquivalentTo(newSize);
            editedDog.BirthDate.ShouldBeEquivalentTo(newBirthDate.ToUniversalTime());
            editedDog.About.ShouldBeEquivalentTo(newAbout);
            editedDog.Row.ShouldBeEquivalentTo(newRow);
            editedDog.Enclosure.ShouldBeEquivalentTo(newEnclosure);
            editedDog.UpdatedBy.ShouldBeEquivalentTo(updatedBy);
            editedDog.LastUpdate.ShouldNotBeSameAs(lastUpdate);
        }

        [Fact]
        public async Task DogInfoShouldNotBeChangedIfUnauthorized()
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
                BirthDate = birthDate.ToUniversalTime(),
                About = about,
                Row = row,
                Enclosure = enclosure,
                LastUpdate = lastUpdate,
                UpdatedBy = user
            };
            _ = await _dbContext.Doges.AddAsync(dog);
            _ = await _dbContext.SaveChangesAsync();

            string newName = Guid.NewGuid().ToString();
            string newBreed = Guid.NewGuid().ToString();
            string newSize = _sizes[_random.Next(_sizes.Length)];
            DateTime newBirthDate = RandomDate.GetRandomDate();
            string newAbout = Guid.NewGuid().ToString();
            int newRow = _random.Next(100);
            int newEnclosure = _random.Next(100);
            Dog newDog = new()
            {
                Id = dog.Id,
                Name = newName,
                Breed = newBreed,
                Size = newSize,
                BirthDate = newBirthDate,
                About = newAbout,
                Row = newRow,
                Enclosure = newEnclosure
            };

            EditDogCommand editDogCommand = new()
            {
                Dog = newDog,
                UpdatedBy = ""
            };

            // Act
            EditDogCommandResult result = await _handler.Handle(editDogCommand, CancellationToken.None);

            // Assert
            result.Dog.ShouldBeNull();
            result.Comment.ShouldBeEquivalentTo("Unauthorized");
            Dog editedDog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == dog.Id, CancellationToken.None);
            editedDog.Name.ShouldBeEquivalentTo(name);
            editedDog.Breed.ShouldBeEquivalentTo(breed);
            editedDog.Size.ShouldBeEquivalentTo(size);
            editedDog.BirthDate.ShouldBeEquivalentTo(birthDate.ToUniversalTime());
            editedDog.About.ShouldBeEquivalentTo(about);
            editedDog.Row.ShouldBeEquivalentTo(row);
            editedDog.Enclosure.ShouldBeEquivalentTo(enclosure);
            editedDog.UpdatedBy.ShouldBeEquivalentTo(user);
            editedDog.LastUpdate.ShouldBeEquivalentTo(lastUpdate);
        }

        [Fact]
        public async Task DogInfoShouldNotBeChangededIfDogNotFound()
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
                BirthDate = birthDate.ToUniversalTime(),
                About = about,
                Row = row,
                Enclosure = enclosure,
                LastUpdate = lastUpdate,
                UpdatedBy = user
            };
            _ = await _dbContext.Doges.AddAsync(dog);
            _ = await _dbContext.SaveChangesAsync();

            string newName = Guid.NewGuid().ToString();
            string newBreed = Guid.NewGuid().ToString();
            string newSize = _sizes[_random.Next(_sizes.Length)];
            DateTime newBirthDate = RandomDate.GetRandomDate();
            string newAbout = Guid.NewGuid().ToString();
            int newRow = _random.Next(100);
            int newEnclosure = _random.Next(100);
            string updatedBy = Guid.NewGuid().ToString();
            Dog newDog = new()
            {
                Id = -1,
                Name = newName,
                Breed = newBreed,
                Size = newSize,
                BirthDate = newBirthDate,
                About = newAbout,
                Row = newRow,
                Enclosure = newEnclosure
            };

            EditDogCommand editDogCommand = new()
            {
                Dog = newDog,
                UpdatedBy = updatedBy
            };

            // Act
            EditDogCommandResult result = await _handler.Handle(editDogCommand, CancellationToken.None);

            // Assert
            result.Dog.ShouldBeNull();
            result.Comment.ShouldBeEquivalentTo("Dog not found");
            Dog editedDog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == dog.Id, CancellationToken.None);
            editedDog.Name.ShouldBeEquivalentTo(name);
            editedDog.Breed.ShouldBeEquivalentTo(breed);
            editedDog.Size.ShouldBeEquivalentTo(size);
            editedDog.BirthDate.ShouldBeEquivalentTo(birthDate.ToUniversalTime());
            editedDog.About.ShouldBeEquivalentTo(about);
            editedDog.Row.ShouldBeEquivalentTo(row);
            editedDog.Enclosure.ShouldBeEquivalentTo(enclosure);
            editedDog.UpdatedBy.ShouldBeEquivalentTo(user);
            editedDog.LastUpdate.ShouldBeEquivalentTo(lastUpdate);
        }

        public void Dispose()
        {
            _ = _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}