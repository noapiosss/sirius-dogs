using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Domain.Database;
using System.IO;
using Contracts.Database;
using System.Linq;
using System;

namespace Domain.Commands;

public class DeletePhotoCommand : IRequest<DeletePhotoCommandResult>
{
    public int DogId { get; init; }
    public Stream PhotoStream { get; init; }
    public string RootPath { get; init; }
}

public class DeletePhotoCommandResult
{
    public bool PhotoIsAdded { get; init; }
}

internal class DeletePhotoCommandHandler : IRequestHandler<DeletePhotoCommand, DeletePhotoCommandResult>
{
    private readonly DogesDbContext _dbContext;

    public DeletePhotoCommandHandler(DogesDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<DeletePhotoCommandResult> Handle(DeletePhotoCommand request, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Images.AnyAsync(i => i.DogId == request.DogId, cancellationToken))
        {
            using (var fileStream = new FileStream($"{request.RootPath}\\images\\{request.DogId}\\1.jpg", FileMode.Create))
            {
                request.PhotoStream.Seek(0, SeekOrigin.Begin);
                request.PhotoStream.CopyTo(fileStream);
            }

            var firstImage = new Image
            {
                DogId = request.DogId,
                PhotoPath = $"/images/{request.DogId}/1.jpg"
            };

            await _dbContext.AddAsync(firstImage, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new DeletePhotoCommandResult
            {
                PhotoIsAdded = true
            };
        }

        var lastPhotoIndex = (await _dbContext.Images
            .Where(i => i.DogId == request.DogId)
            .Select(i => i.PhotoPath)
            .MaxAsync(cancellationToken))
            .Split('/').Last()
            .Split('.').First();
        
        using (var fileStream = new FileStream($"{request.RootPath}\\images\\{request.DogId}\\{Int64.Parse(lastPhotoIndex)+1}.jpg", FileMode.Create))
        {
            request.PhotoStream.Seek(0, SeekOrigin.Begin);
            request.PhotoStream.CopyTo(fileStream);
        }


        var image = new Image
        {
            DogId = request.DogId,
            PhotoPath = $"/images/{request.DogId}/{Int64.Parse(lastPhotoIndex)+1}.jpg"
        };

        await _dbContext.AddAsync(image, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeletePhotoCommandResult
        {
            PhotoIsAdded = true
        };
    }
}