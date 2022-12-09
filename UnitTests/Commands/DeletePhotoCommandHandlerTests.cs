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
    public class DeletePhotoCommandHandlerTests : IDisposable
    {
        private readonly DogesDbContext _dbContext;
        private readonly IRequestHandler<DeletePhotoCommand, DeletePhotoCommandResult> _handler;
        private readonly Random _random = new();
        private readonly string[] _sizes = new string[]
            {
            "Extra small",
            "Small",
            "Medium",
            "Large",
            "Extra large"
            };

        public DeletePhotoCommandHandlerTests()
        {
            _dbContext = DbContextHelper.CreateTestDb();
            _handler = new DeletePhotoCommandHandler(_dbContext);

        }

        [Fact]
        public async Task PhotoShouldBeDeleted()
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

            Image randomPhoto = (await _dbContext.Images.Where(i => i.DogId == dog.Id).ToListAsync(CancellationToken.None))[_random.Next(photosCount)];
            string deletedByUser = Guid.NewGuid().ToString();

            DeletePhotoCommand deletePhotoCommand = new()
            {
                DogId = randomPhoto.DogId,
                PhotoPath = randomPhoto.PhotoPath,
                UpdatedBy = deletedByUser
            };

            // Act
            DeletePhotoCommandResult result = await _handler.Handle(deletePhotoCommand, CancellationToken.None);

            // Assert
            result.PhotoIsDeleted.ShouldBeTrue();
            (await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == randomPhoto.DogId, CancellationToken.None)).UpdatedBy.ShouldBeEquivalentTo(deletedByUser);
            (await _dbContext.Images.AnyAsync(i => i.DogId == randomPhoto.DogId && i.PhotoPath == randomPhoto.PhotoPath, CancellationToken.None)).ShouldBeFalse();
        }

        [Fact]
        public async Task PhotoShouldNotBeDeletedIfDogNotFound()
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

            Image randomPhoto = (await _dbContext.Images.Where(i => i.DogId == dog.Id).ToListAsync(CancellationToken.None))[_random.Next(photosCount)];

            DeletePhotoCommand deletePhotoCommand = new()
            {
                DogId = -1,
                PhotoPath = randomPhoto.PhotoPath
            };

            // Act
            DeletePhotoCommandResult result = await _handler.Handle(deletePhotoCommand, CancellationToken.None);

            // Assert
            result.PhotoIsDeleted.ShouldBeFalse();
            result.Comment.ShouldBeEquivalentTo("Dog not found");
            (await _dbContext.Images.Where(i => i.DogId == randomPhoto.DogId).ToListAsync(CancellationToken.None)).Count.ShouldBeEquivalentTo(photosCount);
        }

        [Fact]
        public async Task PhotoShouldNotBeDeletedIfPhotoNotFound()
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

            DeletePhotoCommand deletePhotoCommand = new()
            {
                DogId = dog.Id,
                PhotoPath = "fake photo"
            };

            // Act
            DeletePhotoCommandResult result = await _handler.Handle(deletePhotoCommand, CancellationToken.None);

            // Assert
            result.PhotoIsDeleted.ShouldBeFalse();
            result.Comment.ShouldBeEquivalentTo("Image not found");
            (await _dbContext.Images.Where(i => i.DogId == dog.Id).ToListAsync(CancellationToken.None)).Count.ShouldBeEquivalentTo(photosCount);
        }

        public void Dispose()
        {
            _ = _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}