using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Database;

using Domain.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Domain.Commands
{
    public class AddPhotoCommand : IRequest<AddPhotoCommandResult>
    {
        public int DogId { get; init; }
        public Stream PhotoStream { get; init; }
        public string RootPath { get; init; }
        public string UpdatedBy { get; init; }
    }

    public class AddPhotoCommandResult
    {
        public string PhotoPath { get; init; }
    }

    internal class AddPhotoCommandHandler : IRequestHandler<AddPhotoCommand, AddPhotoCommandResult>
    {
        private readonly DogesDbContext _dbContext;

        public AddPhotoCommandHandler(DogesDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<AddPhotoCommandResult> Handle(AddPhotoCommand request, CancellationToken cancellationToken = default)
        {
            if (!await _dbContext.Images.AnyAsync(i => i.DogId == request.DogId, cancellationToken))
            {
                using (FileStream fileStream = new($"{request.RootPath}\\images\\{request.DogId}\\1.jpg", FileMode.Create))
                {
                    _ = request.PhotoStream.Seek(0, SeekOrigin.Begin);
                    request.PhotoStream.CopyTo(fileStream);
                }

                Image firstImage = new()
                {
                    DogId = request.DogId,
                    PhotoPath = $"/images/{request.DogId}/1.jpg"
                };

                _ = await _dbContext.AddAsync(firstImage, cancellationToken);
                _ = await _dbContext.SaveChangesAsync(cancellationToken);

                return new AddPhotoCommandResult
                {
                    PhotoPath = firstImage.PhotoPath
                };
            }

            System.Collections.Generic.List<string> photoPaths = await _dbContext.Images
                .Where(i => i.DogId == request.DogId)
                .Select(i => i.PhotoPath)
                .ToListAsync(cancellationToken);

            int lastPhotoIndex = 0;
            for (int i = 0; i < photoPaths.Count; ++i)
            {
                int photoIndex = int.Parse(photoPaths[i].Split('/').Last().Split('.').First());
                if (lastPhotoIndex < photoIndex)
                {
                    lastPhotoIndex = photoIndex;
                }
            }

            using (FileStream fileStream = new($"{request.RootPath}\\images\\{request.DogId}\\{lastPhotoIndex + 1}.jpg", FileMode.Create))
            {
                _ = request.PhotoStream.Seek(0, SeekOrigin.Begin);
                request.PhotoStream.CopyTo(fileStream);
            }

            Image image = new()
            {
                DogId = request.DogId,
                PhotoPath = $"/images/{request.DogId}/{lastPhotoIndex + 1}.jpg"
            };

            _ = await _dbContext.AddAsync(image, cancellationToken);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            Dog dog = await _dbContext.Doges.FirstOrDefaultAsync(d => d.Id == request.DogId, cancellationToken);
            dog.LastUpdate = DateTime.UtcNow;
            dog.UpdatedBy = request.UpdatedBy;

            _ = _dbContext.Doges.Update(dog);
            _ = await _dbContext.SaveChangesAsync(cancellationToken);

            return new AddPhotoCommandResult
            {
                PhotoPath = image.PhotoPath
            };
        }
    }
}