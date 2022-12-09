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
using System.Linq;

namespace UnitTests.Commands
{
    public class DeleteDogCommandHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<DeleteDogCommand, DeleteDogCommandResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public DeleteDogCommandHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new DeleteDogCommandHandler(_dbContext);

        }

        [Fact]
        public async Task DogShouldBeDeleted()
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

            int photosCount = _random.Next(10) + 1;
            for (int i = 0; i < photosCount; ++i)
            {
                Image photo = new()
                {
                    DogId = dog.Id,
                    PhotoPath = Guid.NewGuid().ToString()
                };
                _ = await _dbContext.Images.AddAsync(photo);
                _ = await _dbContext.SaveChangesAsync();
            }

            DeleteDogCommand deleteDogCommand = new()
            {
                DogId = dog.Id
            };

            // Act
            DeleteDogCommandResult result = await _handler.Handle(deleteDogCommand, CancellationToken.None);

            // Assert
            result.DeletingIsSuccessful.ShouldBeTrue();
            (await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == dog.Id, CancellationToken.None)).ShouldBeNull();
            (await _dbContext.Images.Where(i => i.DogId == dog.Id).ToListAsync(CancellationToken.None)).ShouldBeEmpty();
        }

        [Fact]
        public async Task DogShouldNotBeDeletedIfDogNotFound()
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

            int photosCount = _random.Next(10) + 1;
            for (int i = 0; i < photosCount; ++i)
            {
                Image photo = new()
                {
                    DogId = dog.Id,
                    PhotoPath = Guid.NewGuid().ToString()
                };
                _ = await _dbContext.Images.AddAsync(photo);
                _ = await _dbContext.SaveChangesAsync();
            }

            DeleteDogCommand deleteDogCommand = new()
            {
                DogId = -1
            };

            // Act
            DeleteDogCommandResult result = await _handler.Handle(deleteDogCommand, CancellationToken.None);

            // Assert
            result.DeletingIsSuccessful.ShouldBeFalse();
            result.Comment.ShouldBeEquivalentTo("Dog not found");
            _ = (await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == dog.Id, CancellationToken.None)).ShouldNotBeNull();
            (await _dbContext.Images.Where(i => i.DogId == dog.Id).ToListAsync(CancellationToken.None)).Count.ShouldBeEquivalentTo(photosCount);
        }

        public void Dispose()
        {
            _ = _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}